using MediatR;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Application.DTOs;
using System;
using System.Linq;
using AjoCoreBackend.Domain.Exceptions;

namespace AjoCoreBackend.Application.Queries.GetMyCycleDetails
{
    public class GetMyCycleDetailsQueryHandler : IRequestHandler<GetMyCycleDetailsQuery, MyCycleDetailsDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetMyCycleDetailsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<MyCycleDetailsDto> Handle(GetMyCycleDetailsQuery request, CancellationToken cancellationToken)
        {
            var userIdString = _currentUserService.UserId;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated properly.");
            }

            var cycle = await _unitOfWork.SavingCycles.GetByIdAsync(request.SavingCycleId);
            if (cycle == null)
            {
                throw new NotFoundException($"Cycle with ID {request.SavingCycleId} not found.");
            }

            if (!cycle.CycleType.ToString().Equals(request.ExpectedCycleType, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"This endpoint is only for {request.ExpectedCycleType} cycles.");
            }

            var members = await _unitOfWork.Repository<SavingCycleMember>().FindAsync(m => m.SavingCycleId == cycle.Id && m.UserId == userId);
            var member = members.FirstOrDefault();

            if (member == null)
            {
                throw new NotFoundException("You are not a member of this saving cycle.");
            }

            var vAccount = member.NombaVirtualAccountId.HasValue ? await _unitOfWork.Repository<NombaVirtualAccount>().GetByIdAsync(member.NombaVirtualAccountId.Value) : null;

            return new MyCycleDetailsDto
            {
                CycleId = cycle.Id,
                Name = cycle.Name,
                CycleType = cycle.CycleType.ToString(),
                ContributionAmount = cycle.ContributionAmount,
                IntervalDays = cycle.IntervalDays,
                Status = cycle.Status.ToString(),
                StartDate = cycle.StartDate,
                EndDate = cycle.EndDate,
                VirtualAccountNumber = vAccount?.AccountNumber,
                VirtualAccountBank = vAccount?.BankName,
                VirtualAccountName = vAccount?.AccountName,
                PayoutOrder = member.PayoutOrder
            };
        }
    }
}
