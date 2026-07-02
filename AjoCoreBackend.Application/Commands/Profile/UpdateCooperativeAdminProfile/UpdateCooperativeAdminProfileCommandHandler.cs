using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Profile;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using AutoMapper;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Profile.UpdateCooperativeAdminProfile
{
    public class UpdateCooperativeAdminProfileCommandHandler : IRequestHandler<UpdateCooperativeAdminProfileCommand, CooperativeAdminProfileDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public UpdateCooperativeAdminProfileCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<CooperativeAdminProfileDto> Handle(UpdateCooperativeAdminProfileCommand request, CancellationToken cancellationToken)
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

            // Update allowed fields
            admin.FirstName = request.FirstName;
            admin.LastName = request.LastName;
            admin.PhoneNumber = request.PhoneNumber;
            admin.UpdatedAt = System.DateTime.UtcNow;

            _unitOfWork.Repository<CooperativeAdmin>().Update(admin);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return _mapper.Map<CooperativeAdminProfileDto>(admin);
        }
    }
}
