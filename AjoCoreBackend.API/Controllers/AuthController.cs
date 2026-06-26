using System.Threading.Tasks;
using AjoCoreBackend.Application.Commands.Auth.Login;
using AjoCoreBackend.Application.Commands.Auth.Register;
using AjoCoreBackend.Application.Commands.Auth.RefreshToken;
using AjoCoreBackend.Application.Commands.Auth.ForgotPassword;
using AjoCoreBackend.Application.Commands.Auth.ResetPassword;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AjoCoreBackend.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            var result = await _mediator.Send(command);
            // In a real app you might just return Ok("Reset email sent"), 
            // but for hackathon testing we return the code.
            return Ok(new { ResetToken = result });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { Success = result });
        }
    }
}
