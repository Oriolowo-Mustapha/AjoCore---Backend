using AjoCoreBackend.Application.DTOs.Profile;
using MediatR;

namespace AjoCoreBackend.Application.Queries.Profile.GetCooperativeAdminProfile
{
    public class GetCooperativeAdminProfileQuery : IRequest<CooperativeAdminProfileDto>
    {
    }
}
