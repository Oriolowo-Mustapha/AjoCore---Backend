using System;
using System.Threading;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Repositories;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Domain.Entities;
using AjoCoreBackend.Domain.Exceptions;
using MediatR;

namespace AjoCoreBackend.Application.Queries.CooperativeGroups.GenerateInviteLink
{
    public class GenerateInviteLinkQueryHandler : IRequestHandler<GenerateInviteLinkQuery, string>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GenerateInviteLinkQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<string> Handle(GenerateInviteLinkQuery request, CancellationToken cancellationToken)
        {
            var userIdString = _currentUserService.UserId;
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated properly.");
            }

            var group = await _unitOfWork.Repository<CooperativeGroup>().GetByIdAsync(request.GroupId);
            
            if (group == null)
            {
                throw new NotFoundException($"CooperativeGroup with ID {request.GroupId} not found.");
            }

            // Only the admin of the group can generate the invite link
            if (group.CooperativeAdminId != userId)
            {
                throw new ForbiddenAccessException("You are not authorized to generate an invite link for this group.");
            }

            // Build the URL based on the provided frontend base URL.
            // Example output: "https://frontend.com/groups/join?groupId=1234-5678..."
            var cleanBaseUrl = request.BaseUrl.TrimEnd('/');
            return $"{cleanBaseUrl}/groups/join?groupId={request.GroupId}";
        }
    }
}
