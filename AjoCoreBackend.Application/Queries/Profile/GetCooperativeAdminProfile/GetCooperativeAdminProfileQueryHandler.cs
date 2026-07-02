using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Profile;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using AutoMapper;
using MediatR;

namespace AjoCoreBackend.Application.Queries.Profile.GetCooperativeAdminProfile
{
    public class GetCooperativeAdminProfileQueryHandler : IRequestHandler<GetCooperativeAdminProfileQuery, CooperativeAdminProfileDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetCooperativeAdminProfileQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<CooperativeAdminProfileDto> Handle(GetCooperativeAdminProfileQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserGuid;
            if (!userId.HasValue)
            {
                throw new ForbiddenAccessException("User not authenticated.");
            }

            var admin = await _unitOfWork.Repository<CooperativeAdmin>().GetByIdAsync(userId.Value, a => a.AdministeredGroups);
            if (admin == null)
            {
                throw new NotFoundException($"Cooperative Admin with ID {userId.Value} not found.");
            }

            return _mapper.Map<CooperativeAdminProfileDto>(admin);
        }
    }
}
