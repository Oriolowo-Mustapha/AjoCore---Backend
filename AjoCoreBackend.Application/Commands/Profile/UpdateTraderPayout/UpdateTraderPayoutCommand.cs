using System;
using AjoCoreBackend.Application.DTOs.Profile;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Profile.UpdateTraderPayout
{
    public class UpdateTraderPayoutCommand : IRequest<TraderProfileDto>
    {
        public string PayoutAccountNumber { get; set; } = string.Empty;
        public string PayoutBankName { get; set; } = string.Empty;
        public string PayoutAccountName { get; set; } = string.Empty;
    }
}
