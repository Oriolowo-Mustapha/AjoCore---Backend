using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AjoCoreBackend.Application.Interfaces.Services;
using AjoCoreBackend.Application.DTOs.Nomba;

namespace AjoCoreBackend.Infrastructure.Services
{
    public class NombaApiClient : INombaApiClient
    {
        private readonly HttpClient _httpClient;

        public NombaApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<CreateSubAccountResponse> CreateSubAccountAsync(CreateSubAccountRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/v1/accounts/sub-accounts", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CreateSubAccountResponse>() 
                   ?? new CreateSubAccountResponse();
        }

        public async Task<CreateVirtualAccountResponse> CreateVirtualAccountAsync(CreateVirtualAccountRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/v1/accounts/virtual", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CreateVirtualAccountResponse>()
                   ?? new CreateVirtualAccountResponse();
        }

        public async Task<BankLookupResponse> LookupBankAccountAsync(BankLookupRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("/v1/transfers/bank/lookup", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BankLookupResponse>()
                   ?? new BankLookupResponse();
        }

        public async Task<BankTransferResponse> ExecuteBankTransferAsync(BankTransferRequest request)
        {
            // Note: Since Nomba expects Kobo, ensure 'request.Amount' is multiplied by 100 before creating the actual JSON payload
            // In a production scenario, you would map this to an internal record struct or use a custom converter.
            // For now, we will send the request directly.
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
