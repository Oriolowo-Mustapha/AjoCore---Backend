
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AjoCoreBackend.Application.Commands.RecordContribution
{
    public class RecordContributionCommandHandler : IRequestHandler<RecordContributionCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IHangfireBackGroundService _hangfireService;
        private readonly INombaApiClient _nombaApiClient;
        private readonly Microsoft.Extensions.Logging.ILogger<RecordContributionCommandHandler> _logger;

        public RecordContributionCommandHandler(
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider,
            IHangfireBackGroundService hangfireService,
            INombaApiClient nombaApiClient,
            Microsoft.Extensions.Logging.ILogger<RecordContributionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _hangfireService = hangfireService;
            _nombaApiClient = nombaApiClient;
            _logger = logger;
        }

        public async Task<Guid> Handle(RecordContributionCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing webhook contribution for RequestId: {RequestId}, Account: {AccountNumber}, Amount: {Amount}", request.WebhookRequestId, request.AccountNumber, request.Amount);

            // 0. Verify Transaction with Nomba (Security best practice)
            if (!string.IsNullOrEmpty(request.TransactionReference))
            {
                try 
                {
                    var verifyResponse = await _nombaApiClient.VerifyTransactionAsync(request.TransactionReference);
                    if (verifyResponse.Status?.ToUpperInvariant() != "SUCCESS")
                    {
                        _logger.LogWarning("Transaction {TxRef} verification failed. Status: {Status}", request.TransactionReference, verifyResponse.Status);
                        throw new DomainException($"Transaction {request.TransactionReference} is not SUCCESS in Nomba system. Status: {verifyResponse.Status}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception during transaction verification for TxRef: {TxRef}", request.TransactionReference);
                    throw;
                }
            }
            else 
            {
                _logger.LogWarning("Transaction reference is empty. Skipping verification.");
            }
            
            // 1. Check Idempotency
            var alreadyProcessed = await _unitOfWork.Ledgers.HasWebhookBeenProcessedAsync(request.WebhookRequestId);
            if (alreadyProcessed)
            {
                throw new DuplicateWebhookException($"Webhook {request.WebhookRequestId} has already been processed.");
            }

            // 2. Find Member by NUBAN
            _logger.LogInformation("Looking for SavingCycleMember with Virtual Account Number: {AccountNumber}", request.AccountNumber);
            var members = await _unitOfWork.SavingCycleMembers.FindAsync(m => 
                m.VirtualAccount != null && 
                m.VirtualAccount.AccountNumber == request.AccountNumber);
                
            var member = members.FirstOrDefault();

            if (member == null)
            {
                _logger.LogWarning("No member found assigned to virtual account {AccountNumber}.", request.AccountNumber);
                throw new NotFoundException($"No member found assigned to virtual account {request.AccountNumber}.");
            }
            
            _logger.LogInformation("Found member with ID: {MemberId} for AccountNumber: {AccountNumber}", member.Id, request.AccountNumber);

            // 3. Fetch Cycle to check required amount
            var cycle = await _unitOfWork.SavingCycles.GetByIdAsync(member.SavingCycleId);
            if (cycle == null)
            {
                throw new NotFoundException($"Cycle not found.");
            }

            // 4. Amount is already in Naira
            var amountInNaira = request.Amount;

            if (amountInNaira < cycle.ContributionAmount)
            {
                var reversal = new ReversalLedger
                {
                    SavingCycleMemberId = member.Id,
                    Amount = amountInNaira,
                    OriginalWebhookRequestId = request.WebhookRequestId,
                    ReversalTxRef = $"rev_web_{request.WebhookRequestId}",
                    Status = TransactionStatus.Pending,
                    TriggeredAt = _dateTimeProvider.UtcNow
                };

                await _unitOfWork.Repository<ReversalLedger>().AddAsync(reversal);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _hangfireService.EnqueTask<IReversalProcessingService>(x => x.ProcessPendingReversalsAsync(reversal.Id));
                
                return Guid.Empty; // Halt execution so we don't record the invalid contribution
            }

            // 5. Record Ledger Entry
            var ledger = new ContributionLedger
            {
                SavingCycleMemberId = member.Id,
                Amount = amountInNaira,
                NombaWebhookRequestId = request.WebhookRequestId,
                PaidAt = _dateTimeProvider.UtcNow
            };

            await _unitOfWork.Ledgers.AddAsync(ledger);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ledger.Id;
        }
    }
}
