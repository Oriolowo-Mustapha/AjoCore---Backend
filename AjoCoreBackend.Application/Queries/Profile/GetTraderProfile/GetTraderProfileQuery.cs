using AjoCoreBackend.Application.DTOs.Profile;
using MediatR;

namespace AjoCoreBackend.Application.Queries.Profile.GetTraderProfile
{
    public class GetTraderProfileQuery : IRequest<TraderProfileDto>
    {
    }
}
