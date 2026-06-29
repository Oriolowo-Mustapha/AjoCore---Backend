using MediatR;
using AjoCoreBackend.Application.DTOs.Auth;

namespace AjoCoreBackend.Application.Commands.Auth.Register
{
    public record RegisterCommand : IRequest<string>
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string PhoneNumber { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string Role { get; init; } = "Trader"; // "Trader" or "CooperativeAdmin"
        public string? Bvn { get; init; }
        public DateTime DateOfBirth { get; init; }
    }
}
