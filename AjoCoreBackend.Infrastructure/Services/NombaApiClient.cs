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
        private readonly string _accountId;
        private readonly string _subAccountId;

        public NombaApiClient(HttpClient httpClient, INombaTokenService tokenService, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _accountId = configuration["Nomba:AccountId"] ?? throw new ArgumentNullException("Nomba AccountId is missing in configuration.");
            _subAccountId = configuration["Nomba:SubAccountId"] ?? throw new ArgumentNullException("Nomba SubAccountId is missing in configuration.");
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
            
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/v1/accounts/virtual/{_subAccountId}");
            httpRequest.Headers.Add("accountId", _accountId);
            httpRequest.Content = JsonContent.Create(request);

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();
            var wrapper = await response.Content.ReadFromJsonAsync<NombaApiResponse<CreateVirtualAccountResponse>>();
            return wrapper?.Data ?? new CreateVirtualAccountResponse();
        }

        public async Task<BankLookupResponse> LookupBankAccountAsync(BankLookupRequest request)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("/v1/transfers/bank/lookup", request);
            response.EnsureSuccessStatusCode();
            var wrapper = await response.Content.ReadFromJsonAsync<NombaApiResponse<BankLookupResponse>>();
            return wrapper?.Data ?? new BankLookupResponse();
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

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/v2/transfers/bank/{_subAccountId}");
            httpRequest.Headers.Add("accountId", _accountId);
            httpRequest.Content = JsonContent.Create(payload);

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();
            var wrapper = await response.Content.ReadFromJsonAsync<NombaApiResponse<BankTransferResponse>>();
            return wrapper?.Data ?? new BankTransferResponse();
        }

        public async Task<FetchBanksResponse> FetchBanksAsync()
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.GetAsync("/v1/transfers/banks");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<FetchBanksResponse>()
                   ?? new FetchBanksResponse();
        }

        public async Task<VerifyTransactionResponse> VerifyTransactionAsync(string transactionReference)
        {
            await SetAuthorizationHeaderAsync();
            
            var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/transactions/accounts/single?transactionRef={transactionReference}");
            request.Headers.Add("accountId", _accountId);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            var wrapper = await response.Content.ReadFromJsonAsync<NombaApiResponse<VerifyTransactionResponse>>();
            return wrapper?.Data ?? new VerifyTransactionResponse();
        }
    }
}
