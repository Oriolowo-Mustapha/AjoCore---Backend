using System;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Auth.UpdatePayoutAccount
{
    public class UpdatePayoutAccountCommandHandler : IRequestHandler<UpdatePayoutAccountCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBankCodeService _bankCodeService;
        private readonly INombaApiClient _nombaApiClient;

        public UpdatePayoutAccountCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IBankCodeService bankCodeService,
            INombaApiClient nombaApiClient)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _bankCodeService = bankCodeService;
            _nombaApiClient = nombaApiClient;
        }

        public async Task<bool> Handle(UpdatePayoutAccountCommand request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.UserId ?? throw new UnauthorizedAccessException());
            
            var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(userId);
            if (trader == null) throw new NotFoundException($"Trader not found");

            // Verify with Nomba to be absolutely sure
            var bankCode = await _bankCodeService.GetBankCodeByNameAsync(request.BankName);
            if (string.IsNullOrWhiteSpace(bankCode))
            {
                throw new DomainException("Invalid bank name provided.");
            }

            var lookupResponse = await _nombaApiClient.LookupBankAccountAsync(new DTOs.Nomba.BankLookupRequest
            {
                AccountNumber = request.AccountNumber,
                BankCode = bankCode
            });

            if (string.IsNullOrWhiteSpace(lookupResponse.AccountName))
            {
                throw new DomainException("Could not verify the account details with the bank.");
            }

            // Update details
            trader.PayoutAccountNumber = request.AccountNumber;
            trader.PayoutBankName = request.BankName;
            trader.PayoutAccountName = lookupResponse.AccountName; // Use the verified name

            _unitOfWork.Repository<Trader>().Update(trader);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
