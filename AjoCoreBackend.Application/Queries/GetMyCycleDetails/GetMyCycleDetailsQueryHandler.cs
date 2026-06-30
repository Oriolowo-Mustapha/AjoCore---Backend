using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Exceptions;
using AutoMapper;
using MediatR;

namespace AjoCoreBackend.Application.Queries.GetMyCycleDetails
{
    public class GetMyCycleDetailsQueryHandler : IRequestHandler<GetMyCycleDetailsQuery, SavingCycleDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetMyCycleDetailsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<SavingCycleDto> Handle(GetMyCycleDetailsQuery request, CancellationToken cancellationToken)
        {
            var userIdString = _currentUserService.UserId;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated properly.");
            }

            var cycle = await _unitOfWork.SavingCycles.GetCycleWithMembersAsync(request.SavingCycleId);

            if (cycle == null)
            {
                throw new NotFoundException($"Saving Cycle with ID {request.SavingCycleId} was not found.");
            }

            // Filter members to ONLY include the logged-in user before mapping
            cycle.Members = cycle.Members.Where(m => m.UserId == userId).ToList();

            if (!cycle.Members.Any())
            {
                throw new ForbiddenAccessException("You are not a member of this saving cycle.");
            }

            return _mapper.Map<SavingCycleDto>(cycle);
        }
    }
}
