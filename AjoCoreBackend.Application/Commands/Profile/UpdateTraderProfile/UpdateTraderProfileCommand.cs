using System;
using AjoCoreBackend.Application.DTOs.Profile;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Profile.UpdateTraderProfile
{
    public class UpdateTraderProfileCommand : IRequest<TraderProfileDto>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? Bvn { get; set; }
        public string? PayoutAccountNumber { get; set; }
        public string? PayoutBankName { get; set; }
        public string? PayoutAccountName { get; set; }
    }
}
