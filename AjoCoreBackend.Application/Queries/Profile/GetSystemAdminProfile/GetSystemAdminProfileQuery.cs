using AjoCoreBackend.Application.DTOs.Profile;
using MediatR;

namespace AjoCoreBackend.Application.Queries.Profile.GetSystemAdminProfile
{
    public class GetSystemAdminProfileQuery : IRequest<SystemAdminProfileDto>
    {
    }
}
