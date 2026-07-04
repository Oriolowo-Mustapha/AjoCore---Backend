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
            // 1. Create the Saving Cycle type from the string input
            var cycleType = System.Enum.Parse<CycleType>(request.CycleType, ignoreCase: true);

            var cycle = new SavingCycle
            {
                Name = request.Name,
                CycleType = cycleType,
                ContributionAmount = request.ContributionAmount,
                IntervalDays = request.IntervalDays,
                Status = CycleStatus.Pending,
                StartDate = null,
                DurationInIntervals = cycleType == CycleType.Asca ? request.DurationInIntervals : null,
                CooperativeGroupId = request.CooperativeGroupId
            };

            // 4. Persist
            await _unitOfWork.SavingCycles.AddAsync(cycle);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return cycle.Id;
        }
    }
}
