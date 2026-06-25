using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Domain.Exceptions;
using AutoMapper;
using MediatR;

namespace AjoCoreBackend.Application.Queries.GetSavingCycleById
{
    public class GetSavingCycleByIdQueryHandler : IRequestHandler<GetSavingCycleByIdQuery, SavingCycleDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetSavingCycleByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SavingCycleDto> Handle(GetSavingCycleByIdQuery request, CancellationToken cancellationToken)
        {
            var cycle = await _unitOfWork.SavingCycles.GetCycleWithMembersAsync(request.SavingCycleId);

            if (cycle == null)
            {
                throw new NotFoundException($"Saving Cycle with ID {request.SavingCycleId} was not found.");
            }

            return _mapper.Map<SavingCycleDto>(cycle);
        }
    }
}
