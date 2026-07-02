using AjoCoreBackend.Application.DTOs.Profile;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Profile.UpdateCooperativeAdminProfile
{
    public class UpdateCooperativeAdminProfileCommand : IRequest<CooperativeAdminProfileDto>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
