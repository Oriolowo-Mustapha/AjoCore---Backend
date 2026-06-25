using MediatR;
using AjoCoreBackend.Application.DTOs.Auth;

namespace AjoCoreBackend.Application.Commands.Auth.Login
{
    public record LoginCommand : IRequest<AuthResponseDto>
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
