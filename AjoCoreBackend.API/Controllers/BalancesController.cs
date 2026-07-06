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
            return Ok(new { systemStats = result });
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

        /// <summary>
        /// Get the actual Nomba subaccount wallet balance. Cooperative Admin only.
        /// </summary>
        [HttpGet("nomba-wallet")]
        [Authorize(Roles = "CooperativeAdmin")]
        public async Task<IActionResult> GetNombaWalletBalance([FromServices] AjoCoreBackend.Application.Interfaces.Services.INombaApiClient nombaApiClient)
        {
            try
            {
                var balanceResponse = await nombaApiClient.FetchAccountBalanceAsync();
                return Ok(balanceResponse);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch Nomba wallet balance", error = ex.Message });
            }
        }

        /// <summary>
        /// Manually withdraw funds from the Nomba subaccount to a designated bank account. Cooperative Admin only.
        /// </summary>
        [HttpPost("withdraw-nomba-funds")]
        [Authorize(Roles = "CooperativeAdmin")]
        public async Task<IActionResult> WithdrawNombaFunds(
            [FromServices] AjoCoreBackend.Application.Interfaces.Services.INombaApiClient nombaApiClient,
            [FromBody] AjoCoreBackend.Application.DTOs.Nomba.BankTransferRequest request)
        {
            try
            {
                var response = await nombaApiClient.ExecuteBankTransferAsync(request);
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Failed to execute Nomba withdrawal", error = ex.Message });
            }
        }
    }
}
