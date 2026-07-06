using System;
using AjoCoreBackend.Application.DTOs.Profile;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Profile.UpdateTraderBvn
{
    public class UpdateTraderBvnCommand : IRequest<TraderProfileDto>
    {
        public string Bvn { get; set; } = string.Empty;
    }
}
