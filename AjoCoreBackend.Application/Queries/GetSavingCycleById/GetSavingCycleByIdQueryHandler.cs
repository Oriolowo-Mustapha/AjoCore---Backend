using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using AutoMapper;
using MediatR;

namespace AjoCoreBackend.Application.Queries.GetSavingCycleById
{
    public class GetSavingCycleByIdQueryHandler : IRequestHandler<GetSavingCycleByIdQuery, SavingCycleDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetSavingCycleByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<SavingCycleDto> Handle(GetSavingCycleByIdQuery request, CancellationToken cancellationToken)
        {
            var cycle = await _unitOfWork.SavingCycles.GetCycleWithMembersAsync(request.SavingCycleId);

            if (cycle == null)
            {
                throw new NotFoundException($"Saving Cycle with ID {request.SavingCycleId} was not found.");
            }

            var dto = _mapper.Map<SavingCycleDto>(cycle);

            // Hide virtual account details if the logged in user is NOT a CooperativeAdmin
            bool isAdmin = false;
            if (Guid.TryParse(_currentUserService.UserId, out Guid userId))
            {
                var admin = await _unitOfWork.Repository<AjoCoreBackend.Domain.Entities.CooperativeAdmin>().GetByIdAsync(userId);
                if (admin != null) isAdmin = true;
            }

            if (!isAdmin)
            {
                var safeMembers = dto.Members.Select(m => m with 
                { 
                    VirtualAccountNumber = null, 
                    VirtualAccountBank = null 
                }).ToList();
                
                dto = dto with { Members = safeMembers };
            }

            return dto;
        }
    }
}
