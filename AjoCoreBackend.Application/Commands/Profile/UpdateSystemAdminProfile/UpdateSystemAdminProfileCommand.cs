using System;
using AjoCoreBackend.Application.DTOs.Profile;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Profile.UpdateSystemAdminProfile
{
    public class UpdateSystemAdminProfileCommand : IRequest<SystemAdminProfileDto>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
