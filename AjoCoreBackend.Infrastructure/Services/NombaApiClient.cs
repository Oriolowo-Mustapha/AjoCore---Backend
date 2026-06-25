using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Application.DTOs.Nomba;

namespace AjoCoreBackend.Infrastructure.Services
{
    public class NombaApiClient : INombaApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly INombaTokenService _tokenService;

        public NombaApiClient(HttpClient httpClient, INombaTokenService tokenService)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var token = await _tokenService.GetAccessTokenAsync();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<CreateSubAccountResponse> CreateSubAccountAsync(CreateSubAccountRequest request)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("/v1/accounts/sub-accounts", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CreateSubAccountResponse>() 
                   ?? new CreateSubAccountResponse();
        }

        public async Task<CreateVirtualAccountResponse> CreateVirtualAccountAsync(CreateVirtualAccountRequest request)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("/v1/accounts/virtual", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CreateVirtualAccountResponse>()
                   ?? new CreateVirtualAccountResponse();
        }

        public async Task<BankLookupResponse> LookupBankAccountAsync(BankLookupRequest request)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("/v1/transfers/bank/lookup", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BankLookupResponse>()
                   ?? new BankLookupResponse();
        }

        public async Task<BankTransferResponse> ExecuteBankTransferAsync(BankTransferRequest request)
        {
            await SetAuthorizationHeaderAsync();
            
            var payload = new
            {
                amount = (long)(request.Amount * 100), // Convert decimal Naira to integer Kobo
                accountNumber = request.AccountNumber,
                accountName = request.AccountName,
                bankCode = request.BankCode,
                merchantTxRef = request.MerchantTxRef,
                senderName = request.SenderName
            };

            var response = await _httpClient.PostAsJsonAsync("/v1/transfers/bank", payload);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BankTransferResponse>()
                   ?? new BankTransferResponse();
        }
    }
}
