using AjoCoreBackend.Application.Queries.Balances.GetCooperativeAdminBalance;
using AjoCoreBackend.Application.Queries.Balances.GetSystemAdminBalance;
using AjoCoreBackend.Application.Queries.Balances.GetTraderBalance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AjoCoreBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalancesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BalancesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get the total system wallet balance. System Admin only.
        /// </summary>
        [HttpGet("system")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> GetSystemBalance()
        {
            var result = await _mediator.Send(new GetSystemAdminBalanceQuery());
            return Ok(result);
        }

        /// <summary>
        /// Get the total balance for a specific cooperative group. Cooperative Admin only.
        /// </summary>
        [HttpGet("cooperative/{groupId}")]
        [Authorize(Roles = "CooperativeAdmin")]
        public async Task<IActionResult> GetCooperativeBalance(Guid groupId)
        {
            var result = await _mediator.Send(new GetCooperativeAdminBalanceQuery { CooperativeGroupId = groupId });
            return Ok(result);
        }

        /// <summary>
        /// Get the total balance for a specific saving cycle. Cooperative Admin only.
        /// </summary>
        [HttpGet("cycle/{cycleId}")]
        [Authorize(Roles = "CooperativeAdmin")]
        public async Task<IActionResult> GetCycleBalance(Guid cycleId)
        {
            var result = await _mediator.Send(new AjoCoreBackend.Application.Queries.Balances.GetSavingCycleBalance.GetSavingCycleBalanceQuery { SavingCycleId = cycleId });
            return Ok(result);
        }

        /// <summary>
        /// Get the logged-in trader's balance across all their saving cycles.
        /// </summary>
        [HttpGet("my-balances")]
        [Authorize]
        public async Task<IActionResult> GetMyBalances()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var traderId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new GetTraderBalanceQuery { TraderId = traderId });
            return Ok(result);
        }
    }
}
