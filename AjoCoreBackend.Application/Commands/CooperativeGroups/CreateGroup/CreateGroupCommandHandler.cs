using System;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.CreateGroup
{
    public class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateGroupCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
        {
            var userId = Guid.Parse(_currentUserService.UserId 
                ?? throw new ForbiddenAccessException("You must be logged in."));

            // Verify the user is a CooperativeAdmin
            var admins = await _unitOfWork.Repository<CooperativeAdmin>().FindAsync(a => a.Id == userId);
            var admin = System.Linq.Enumerable.FirstOrDefault(admins);

            if (admin == null || admin.Role != UserRole.CooperativeAdmin)
            {
                throw new ForbiddenAccessException("Only Cooperative Admins can create groups.");
            }

            var group = new CooperativeGroup
            {
                Name = request.Name,
                Description = request.Description,
                CooperativeAdminId = userId
            };

            await _unitOfWork.Repository<CooperativeGroup>().AddAsync(group);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return group.Id;
        }
    }
}
