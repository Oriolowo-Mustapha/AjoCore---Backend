using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Application.DTOs;
using System.Linq;
using AjoCoreBackend.Domain.Exceptions;

namespace AjoCoreBackend.Application.Queries.GetSavingCycleMembers
{
    public class GetSavingCycleMembersQueryHandler : IRequestHandler<GetSavingCycleMembersQuery, List<SavingCycleMemberDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetSavingCycleMembersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<SavingCycleMemberDto>> Handle(GetSavingCycleMembersQuery request, CancellationToken cancellationToken)
        {
            var cycle = await _unitOfWork.SavingCycles.GetCycleWithMembersAsync(request.SavingCycleId);

            if (cycle == null)
            {
                throw new NotFoundException($"Cycle with ID {request.SavingCycleId} not found.");
            }

            var memberDtos = new List<SavingCycleMemberDto>();

            foreach (var member in cycle.Members)
            {
                var vAccount = await _unitOfWork.Repository<NombaVirtualAccount>().GetByIdAsync(member.NombaVirtualAccountId);
                var trader = await _unitOfWork.Repository<Trader>().GetByIdAsync(member.UserId);
                var contributions = await _unitOfWork.Repository<ContributionLedger>().FindAsync(c => c.SavingCycleMemberId == member.Id);
                var totalContributed = contributions.Sum(c => c.Amount);

                memberDtos.Add(new SavingCycleMemberDto
                {
                    Id = member.Id,
                    SavingCycleId = cycle.Id,
                    VirtualAccountNumber = vAccount?.AccountNumber,
                    VirtualAccountBank = vAccount?.BankName,
                    PayoutOrder = member.PayoutOrder,
                    Status = member.Status.ToString(),
                    JoinedAt = member.CreatedAt,
                    UserId = member.UserId,
                    TraderName = trader != null ? $"{trader.FirstName} {trader.LastName}" : "Unknown",
                    TraderEmail = trader?.Email ?? "",
                    TotalContributed = totalContributed
                });
            }

            return memberDtos.OrderBy(m => m.PayoutOrder).ToList();
        }
    }
}
