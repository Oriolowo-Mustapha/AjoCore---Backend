using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Application.DTOs.Nomba;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System;
using AjoCoreBackend.Domain.Exceptions;

namespace AjoCoreBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BanksController : ControllerBase
    {
        private readonly IBankCodeService _bankCodeService;
        private readonly INombaApiClient _nombaApiClient;

        public BanksController(IBankCodeService bankCodeService, INombaApiClient nombaApiClient)
        {
            _bankCodeService = bankCodeService;
            _nombaApiClient = nombaApiClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetBanks()
        {
            var banks = await _bankCodeService.GetAllBanksAsync();
            return Ok(new { success = true, data = banks.OrderBy(b => b.BankName) });
        }

        [HttpGet("lookup")]
        public async Task<IActionResult> LookupBank([FromQuery] string accountNumber, [FromQuery] string bankName)
        {
            if (string.IsNullOrWhiteSpace(accountNumber) || string.IsNullOrWhiteSpace(bankName))
            {
                return BadRequest(new { success = false, message = "Account number and bank name are required." });
            }

            var bankCode = await _bankCodeService.GetBankCodeByNameAsync(bankName);
            if (string.IsNullOrWhiteSpace(bankCode))
            {
                return BadRequest(new { success = false, message = "Invalid bank name provided." });
            }

                var response = await _nombaApiClient.LookupBankAccountAsync(new BankLookupRequest
                {
                    AccountNumber = accountNumber,
                    BankCode = bankCode
                });

                if (response == null || string.IsNullOrWhiteSpace(response.AccountName))
                {
                    throw new NotFoundException("Account details not found with the provider.");
                }

                return Ok(new { success = true, data = response });
        }
    }
}
