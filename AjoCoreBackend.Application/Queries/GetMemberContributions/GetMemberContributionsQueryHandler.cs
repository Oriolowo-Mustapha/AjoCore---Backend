using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace AjoCoreBackend.Application.Queries.GetMemberContributions
{
    public class GetMemberContributionsQueryHandler : IRequestHandler<GetMemberContributionsQuery, List<ContributionLedgerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetMemberContributionsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ContributionLedgerDto>> Handle(GetMemberContributionsQuery request, CancellationToken cancellationToken)
        {
            var contributions = await _unitOfWork.Ledgers.FindAsync(l => l.SavingCycleMemberId == request.SavingCycleMemberId);
            return _mapper.Map<List<ContributionLedgerDto>>(contributions);
        }
    }
}
