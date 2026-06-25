using System;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Nomba;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CreateSavingCycle
{
    public class CreateSavingCycleCommandHandler : IRequestHandler<CreateSavingCycleCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INombaApiClient _nombaApiClient;
        private readonly IDateTimeProvider _dateTimeProvider;

        public CreateSavingCycleCommandHandler(
            IUnitOfWork unitOfWork,
            INombaApiClient nombaApiClient,
            IDateTimeProvider dateTimeProvider)
        {
            _unitOfWork = unitOfWork;
            _nombaApiClient = nombaApiClient;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<Guid> Handle(CreateSavingCycleCommand request, CancellationToken cancellationToken)
        {
            // 1. Provision an isolated Nomba sub-account for this cycle's funds
            var subAccountResponse = await _nombaApiClient.CreateSubAccountAsync(new CreateSubAccountRequest
            {
                AccountName = $"AjoCore_Cycle_{request.Name}",
                Email = $"cycle_{Guid.NewGuid():N}@ajocore.internal"
            });

            // 2. Parse the cycle type from the string input
            var cycleType = System.Enum.Parse<CycleType>(request.CycleType, ignoreCase: true);

            // 3. Build the domain entity
            var cycle = new SavingCycle
            {
                Name = request.Name,
                CycleType = cycleType,
                ContributionAmount = request.ContributionAmount,
                IntervalDays = request.IntervalDays,
                NombaSubAccountId = subAccountResponse.SubAccountId,
                Status = CycleStatus.Pending,
                StartDate = _dateTimeProvider.UtcNow,
                CooperativeGroupId = request.CooperativeGroupId
            };

            // 4. Persist
            await _unitOfWork.SavingCycles.AddAsync(cycle);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return cycle.Id;
        }
    }
}
