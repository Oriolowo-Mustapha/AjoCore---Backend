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

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GetGroupPayouts
{
    public class GetGroupPayoutsQueryHandler : IRequestHandler<GetGroupPayoutsQuery, List<GroupPayoutLedgerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetGroupPayoutsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<GroupPayoutLedgerDto>> Handle(GetGroupPayoutsQuery request, CancellationToken cancellationToken)
        {
            var group = await _unitOfWork.Repository<CooperativeGroup>().GetByIdAsync(request.GroupId);
            if (group == null) throw new NotFoundException("Group not found");

            if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId) || group.CooperativeAdminId != currentUserId)
            {
                throw new UnauthorizedAccessException("Only the group admin can view ledger.");
            }

            var cycles = await _unitOfWork.Repository<SavingCycle>()
                .FindAsync(c => c.CooperativeGroupId == request.GroupId);
            var cycleIds = cycles.Select(c => c.Id).ToList();

            var cycleMembers = await _unitOfWork.Repository<SavingCycleMember>()
                .FindAsync(m => cycleIds.Contains(m.SavingCycleId));
            var cycleMemberIds = cycleMembers.Select(m => m.Id).ToList();

            IEnumerable<PayoutLedger> payouts = await _unitOfWork.Repository<PayoutLedger>()
                .FindAsync(p => cycleMemberIds.Contains(p.SavingCycleMemberId));

            if (request.FromDate.HasValue)
                payouts = payouts.Where(p => p.CreatedAt >= request.FromDate.Value);
            
            if (request.ToDate.HasValue)
                payouts = payouts.Where(p => p.CreatedAt <= request.ToDate.Value);

            var result = new List<GroupPayoutLedgerDto>();
            foreach (var p in payouts)
            {
                var member = cycleMembers.First(m => m.Id == p.SavingCycleMemberId);
                var cycle = cycles.First(c => c.Id == member.SavingCycleId);
                var trader = member != null ? await _unitOfWork.Repository<Trader>().GetByIdAsync(member.UserId) : null;
                
                result.Add(new GroupPayoutLedgerDto
                {
                    Id = p.Id,
                    PayoutDate = p.CreatedAt,
                    Amount = p.Amount,
                    MerchantTxRef = p.MerchantTxRef,
                    MemberName = trader != null ? $"{trader.FirstName} {trader.LastName}" : "Unknown",
                    CycleName = cycle.Name
                });
            }

            return result.OrderByDescending(x => x.PayoutDate).ToList();
        }
    }
}
