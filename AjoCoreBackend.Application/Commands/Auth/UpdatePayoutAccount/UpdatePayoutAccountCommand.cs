using MediatR;
using System;

namespace AjoCoreBackend.Application.Commands.Auth.UpdatePayoutAccount
{
    public class UpdatePayoutAccountCommand : IRequest<bool>
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
    }
}
