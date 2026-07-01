
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.RecordContribution
{
    public class RecordContributionCommandHandler : IRequestHandler<RecordContributionCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IHangfireBackGroundService _hangfireService;
        private readonly IReversalProcessingService _reversalProcessingService;

        public RecordContributionCommandHandler(
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTimeProvider,
            IHangfireBackGroundService hangfireService)
        {
            _unitOfWork = unitOfWork;
            _dateTimeProvider = dateTimeProvider;
            _hangfireService = hangfireService;
        }

        public async Task<Guid> Handle(RecordContributionCommand request, CancellationToken cancellationToken)
        {
            // 1. Check Idempotency
            var alreadyProcessed = await _unitOfWork.Ledgers.HasWebhookBeenProcessedAsync(request.WebhookRequestId);
            if (alreadyProcessed)
            {
                throw new DuplicateWebhookException($"Webhook {request.WebhookRequestId} has already been processed.");
            }

            // 2. Find Member by NUBAN
            var members = await _unitOfWork.SavingCycleMembers.FindAsync(m => 
                m.VirtualAccount != null && 
                m.VirtualAccount.AccountNumber == request.AccountNumber);
                
            var member = members.FirstOrDefault();

            if (member == null)
            {
                throw new NotFoundException($"No member found assigned to virtual account {request.AccountNumber}.");
            }

            // 3. Fetch Cycle to check required amount
            var cycle = await _unitOfWork.SavingCycles.GetByIdAsync(member.SavingCycleId);
            if (cycle == null)
            {
                throw new NotFoundException($"Cycle not found.");
            }

            // 4. Convert Kobo to Naira
            var amountInNaira = request.Amount / 100m;

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
