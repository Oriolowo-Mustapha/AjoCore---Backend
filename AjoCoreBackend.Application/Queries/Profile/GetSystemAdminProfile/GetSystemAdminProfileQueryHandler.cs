using System;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Profile;
using AjoCoreBackend.Domain.Exceptions;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using MediatR;

namespace AjoCoreBackend.Application.Queries.Profile.GetSystemAdminProfile
{
    public class GetSystemAdminProfileQueryHandler : IRequestHandler<GetSystemAdminProfileQuery, SystemAdminProfileDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetSystemAdminProfileQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<SystemAdminProfileDto> Handle(GetSystemAdminProfileQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                throw new UnauthorizedAccessException("Invalid user ID.");
            }

            var admin = await _unitOfWork.Repository<SystemAdmin>().GetByIdAsync(parsedUserId);
            if (admin == null)
            {
                throw new NotFoundException($"SystemAdmin with ID {parsedUserId} not found.");
            }

            return new SystemAdminProfileDto
            {
                Id = admin.Id,
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                Email = admin.Email,
                Role = admin.Role.ToString()
            };
        }
    }
}
