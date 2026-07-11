using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Enum;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Commands.CooperativeGroups.DeactivateGroup
{
    public class DeactivateGroupCommandHandler : IRequestHandler<DeactivateGroupCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeactivateGroupCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(DeactivateGroupCommand request, CancellationToken cancellationToken)
        {
            var group = await _unitOfWork.Repository<CooperativeGroup>().GetByIdAsync(request.GroupId);
            if (group == null)
            {
                throw new NotFoundException($"Cooperative group with ID {request.GroupId} not found.");
            }

            if (!Guid.TryParse(_currentUserService.UserId, out var currentUserId) || group.CooperativeAdminId != currentUserId)
            {
                throw new UnauthorizedAccessException("Only the group admin can deactivate this group.");
            }

            var activeCycles = await _unitOfWork.Repository<SavingCycle>()
                .FindAsync(c => c.CooperativeGroupId == request.GroupId && c.Status == CycleStatus.Active);

            if (activeCycles.Any())
            {
                throw new InvalidOperationException("Cannot deactivate group: there are active saving cycles. Complete or cancel all active cycles first.");
            }

            group.Status = GroupStatus.Inactive;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
