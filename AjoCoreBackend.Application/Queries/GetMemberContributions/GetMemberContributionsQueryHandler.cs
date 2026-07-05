using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace AjoCoreBackend.Application.Queries.GetMemberContributions
{
    public class GetMemberContributionsQueryHandler : IRequestHandler<GetMemberContributionsQuery, MemberContributionsResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetMemberContributionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MemberContributionsResponseDto> Handle(GetMemberContributionsQuery request, CancellationToken cancellationToken)
        {
            var member = await _unitOfWork.SavingCycleMembers.GetByIdAsync(request.SavingCycleMemberId);
            var contributions = await _unitOfWork.Ledgers.FindAsync(l => l.SavingCycleMemberId == request.SavingCycleMemberId);
            
            var response = new MemberContributionsResponseDto
            {
                MemberName = member != null && member.Trader != null ? $"{member.Trader.FirstName} {member.Trader.LastName}" : string.Empty,
                TotalContributed = contributions != null ? contributions.Sum(c => c.Amount) : 0,
                Contributions = _mapper.Map<List<ContributionLedgerDto>>(contributions)
            };

            return response;
        }
    }
}
