using MediatR;

namespace AjoCoreBackend.Application.Commands.Auth.ResetPassword
{
    public record ResetPasswordCommand : IRequest<bool>
    {
        public string Email { get; init; } = string.Empty;
        public string Token { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
    }
}
