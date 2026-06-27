using AjoCoreBackend.Application.DTOs.Nomba;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace AjoCoreBackend.Application.Commands.IndividualContriution.CreateIndividualSavingCycle
{
    public class CreateIndividualSavingCycleHandler(IUnitOfWork _unitOfWork,ILogger<CreateIndividualSavingCycleHandler> _logger,INombaApiClient _nombaApiClient,IDateTimeProvider _dateTimeProvider, ICurrentUserService _currentUser) : IRequestHandler<CreateIndividualSavingCycleCommand, Guid>
    {
        public async Task<Guid> Handle(CreateIndividualSavingCycleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating a new saving cycle with name: {CycleName}", request.Name);

            var subAccountResponse =  await _nombaApiClient.CreateSubAccountAsync(new CreateSubAccountRequest
            {

                AccountName = $"AjoCore_Cycle_{request.Name}",
                Email = $"cycle_{Guid.NewGuid():N}@ajocore.internal"
            });
             var savingCyleType = Enum.Parse<CycleType>(request.CycleType, ignoreCase: true);

            _logger.LogInformation("Successfully created Nomba Sub-Account with ID: {SubAccountId}", subAccountResponse.SubAccountId);
            var newSavingCycle = new SavingCycle
            {
                Name = request.Name,
                CycleType = savingCyleType,
                ContributionAmount = request.ContributionAmount,
                IntervalDays = request.IntervalDays,
                NombaSubAccountId = subAccountResponse.SubAccountId,
                Status = CycleStatus.Pending,
                StartDate = _dateTimeProvider.UtcNow,
                CooperativeGroupId = request.CooperativeGroupId,
                IndividualOwnerId = request.IndividualOwnerId
            };
            var userId = _currentUser.UserId ?? throw new KeyNotFoundException($"Current user with ID {_currentUser.UserId} not found");
            // Provision a NUBAN Virtual Account attached to the cycle's Nomba Sub-Account
            var virtualAccountResponse = await _nombaApiClient.CreateVirtualAccountAsync(new CreateVirtualAccountRequest
            {
                SubAccountId = newSavingCycle.NombaSubAccountId,
                AccountReference = $"member_{userId :N}"
            });

            _logger.LogInformation("Successfully created Nomba Virtual Account with Number: {AccountNumber}", virtualAccountResponse.AccountNumber);

            var virtualAccount = new NombaVirtualAccount
            {
                SubAccountId = newSavingCycle.NombaSubAccountId,
                AccountNumber = virtualAccountResponse.AccountNumber,
                BankName = virtualAccountResponse.BankName,
                AccountName = virtualAccountResponse.AccountName
            };

            var savingCycleMember = new SavingCycleMember
            {
                UserId = (Guid) _currentUser.UserGuid!,
                SavingCycleId = newSavingCycle.Id,
                NombaVirtualAccountId = virtualAccount.Id
            };
            await _unitOfWork.SavingCycles.AddAsync(newSavingCycle);

            await _unitOfWork.Repository<NombaVirtualAccount>().AddAsync(virtualAccount);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return newSavingCycle.Id;
        }
    }
}
