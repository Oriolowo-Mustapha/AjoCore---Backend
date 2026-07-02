using System.Threading.Tasks;
using AjoCoreBackend.Application.Commands.Profile.UpdateCooperativeAdminProfile;
using AjoCoreBackend.Application.Commands.Profile.UpdateTraderProfile;
using AjoCoreBackend.Application.DTOs.Profile;
using AjoCoreBackend.Application.Queries.Profile.GetCooperativeAdminProfile;
using AjoCoreBackend.Application.Queries.Profile.GetTraderProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AjoCoreBackend.API.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get the currently logged-in Trader's profile.
        /// </summary>
        [HttpGet("trader")]
        [Authorize(Roles = "Trader")]
        [ProducesResponseType(typeof(TraderProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTraderProfile()
        {
            var result = await _mediator.Send(new GetTraderProfileQuery());
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// Update the currently logged-in Trader's profile.
        /// </summary>
        [HttpPut("trader")]
        [Authorize(Roles = "Trader")]
        [ProducesResponseType(typeof(TraderProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTraderProfile([FromBody] UpdateTraderProfileCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// Get the currently logged-in Cooperative Admin's profile.
        /// </summary>
        [HttpGet("cooperative-admin")]
        [Authorize(Roles = "CooperativeAdmin")]
        [ProducesResponseType(typeof(CooperativeAdminProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCooperativeAdminProfile()
        {
            var result = await _mediator.Send(new GetCooperativeAdminProfileQuery());
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// Update the currently logged-in Cooperative Admin's profile.
        /// </summary>
        [HttpPut("cooperative-admin")]
        [Authorize(Roles = "CooperativeAdmin")]
        [ProducesResponseType(typeof(CooperativeAdminProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCooperativeAdminProfile([FromBody] UpdateCooperativeAdminProfileCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { success = true, data = result });
        }
    }
}
