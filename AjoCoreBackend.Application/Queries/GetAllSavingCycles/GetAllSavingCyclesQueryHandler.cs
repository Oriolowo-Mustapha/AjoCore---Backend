using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace AjoCoreBackend.Application.Queries.GetAllSavingCycles
{
    public class GetAllSavingCyclesQueryHandler : IRequestHandler<GetAllSavingCyclesQuery, List<SavingCycleDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAllSavingCyclesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<SavingCycleDto>> Handle(GetAllSavingCyclesQuery request, CancellationToken cancellationToken)
        {
            var cycles = await _unitOfWork.SavingCycles.GetAllAsync();
            return _mapper.Map<List<SavingCycleDto>>(cycles);
        }
    }
}
