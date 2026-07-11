using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetGroupContributions
{
    public class GetGroupContributionsQueryHandler : IRequestHandler<GetGroupContributionsQuery, List<GroupContributionLedgerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetGroupContributionsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<GroupContributionLedgerDto>> Handle(GetGroupContributionsQuery request, CancellationToken cancellationToken)
        {
            var group = await _unitOfWork.Repository<CooperativeGroup>().GetByIdAsync(request.GroupId);
            if (group == null) throw new NotFoundException("Group not found");

            if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId) || group.CooperativeAdminId != currentUserId)
            {
                throw new UnauthorizedAccessException("Only the group admin can view ledger.");
            }

            IEnumerable<SavingCycle> cycles = await _unitOfWork.Repository<SavingCycle>()
                .FindAsync(c => c.CooperativeGroupId == request.GroupId);
            
            if (request.CycleId.HasValue)
            {
                cycles = cycles.Where(c => c.Id == request.CycleId.Value);
            }
            
            var cycleIds = cycles.Select(c => c.Id).ToList();

            var cycleMembers = await _unitOfWork.Repository<SavingCycleMember>()
                .FindAsync(m => cycleIds.Contains(m.SavingCycleId));
            var cycleMemberIds = cycleMembers.Select(m => m.Id).ToList();

            IEnumerable<ContributionLedger> contributions = await _unitOfWork.Repository<ContributionLedger>()
                .FindAsync(c => cycleMemberIds.Contains(c.SavingCycleMemberId));

            if (request.FromDate.HasValue)
                contributions = contributions.Where(c => c.PaidAt >= request.FromDate.Value);
            
            if (request.ToDate.HasValue)
                contributions = contributions.Where(c => c.PaidAt <= request.ToDate.Value);

            var result = new List<GroupContributionLedgerDto>();
            foreach (var c in contributions)
            {
                var member = cycleMembers.First(m => m.Id == c.SavingCycleMemberId);
                var cycle = cycles.First(cy => cy.Id == member.SavingCycleId);
                var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(member.UserId);
                
                result.Add(new GroupContributionLedgerDto
                {
                    Id = c.Id,
                    PaidAt = c.PaidAt,
                    Amount = c.Amount,
                    MemberName = trader != null ? $"{trader.FirstName} {trader.LastName}" : "Unknown",
                    CycleName = cycle.Name,
                    WebhookId = c.NombaWebhookRequestId
                });
            }

            return result.OrderByDescending(x => x.PaidAt).ToList();
        }
    }
}
