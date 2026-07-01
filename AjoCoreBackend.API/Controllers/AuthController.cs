using System.Threading.Tasks;
using AjoCoreBackend.Application.Commands.Auth.Login;
using AjoCoreBackend.Application.Commands.Auth.Register;
using AjoCoreBackend.Application.Commands.Auth.RefreshToken;
using AjoCoreBackend.Application.Commands.Auth.ForgotPassword;
using AjoCoreBackend.Application.Commands.Auth.ResetPassword;
using AjoCoreBackend.Application.Commands.Auth.VerifyEmail;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using AjoCoreBackend.Application.Commands.Auth.UpdateBvn;
using AjoCoreBackend.Application.Commands.Auth.UpdatePayoutAccount;
using AjoCoreBackend.Application.DTOs.Auth;
using AjoCoreBackend.Application.Commands.Auth.AdminLogin;

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
            return Ok(new { Message = result });
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { Success = result });
        }

        /// <summary>
        /// Login a user and return a JWT token.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Login a System Admin and return a JWT token.
        /// </summary>
        [HttpPost("admin-login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AdminLogin(AdminLoginCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Refresh an expired JWT token using a valid refresh token.
        /// </summary>
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

        [Authorize(Roles = "Trader")]
        [HttpPost("update-bvn")]
        public async Task<IActionResult> UpdateBvn([FromBody] UpdateBvnCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { Success = result });
        }

        [Authorize]
        [HttpPut("payout-account")]
        public async Task<IActionResult> UpdatePayoutAccount([FromBody] UpdatePayoutAccountCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { Success = result });
        }
    }
}
