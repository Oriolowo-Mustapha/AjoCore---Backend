using AjoCoreBackend.Application.DTOs.Auth;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Auth.RefreshToken
{
    public record RefreshTokenCommand : IRequest<AuthResponseDto>
    {
        public string Token { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
    }
}
