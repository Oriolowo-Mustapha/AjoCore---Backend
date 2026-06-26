using MediatR;

namespace AjoCoreBackend.Application.Commands.Auth.ForgotPassword
{
    public record ForgotPasswordCommand : IRequest<string>
    {
        public string Email { get; init; } = string.Empty;
    }
}
