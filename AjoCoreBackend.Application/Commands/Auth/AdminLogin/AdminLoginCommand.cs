using AjoCoreBackend.Application.DTOs.Auth;
using MediatR;

namespace AjoCoreBackend.Application.Commands.Auth.AdminLogin
{
    public class AdminLoginCommand : IRequest<AuthResponseDto>
    {
        public string EmailOrUsername { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
