using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using System.Linq;
using System;
using AjoCoreBackend.Application.DTOs;

namespace AjoCoreBackend.Application.Queries.GetMyPersonalCycles
{
    public class GetMyPersonalCyclesQueryHandler : IRequestHandler<GetMyPersonalCyclesQuery, List<SavingCycleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetMyPersonalCyclesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<SavingCycleDto>> Handle(GetMyPersonalCyclesQuery request, CancellationToken cancellationToken)
        {
            var userIdString = _currentUserService.UserId;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated properly.");
            }

            var allCycles = await _unitOfWork.Repository<SavingCycle>().FindAsync(c => c.CycleType == CycleType.Personal && c.CooperativeGroupId == null);
            var personalCycles = new List<SavingCycleDto>();
            
            foreach (var c in allCycles)
            {
                var cycleMembers = await _unitOfWork.Repository<SavingCycleMember>().FindAsync(m => m.SavingCycleId == c.Id && m.UserId == userId);
                if (cycleMembers.Any())
                {
                    personalCycles.Add(new SavingCycleDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        CycleType = c.CycleType.ToString(),
                        ContributionAmount = c.ContributionAmount,
                        IntervalDays = c.IntervalDays,
                        TargetAmount = c.IndividualTargetAmount,
                        StartDate = c.StartDate,
                        EndDate = c.EndDate,
                        Status = c.Status.ToString()
                    });
                }
            }

            return personalCycles;
        }
    }
}
