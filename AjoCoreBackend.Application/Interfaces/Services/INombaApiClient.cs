using System.Threading.Tasks;
using AjoCoreBackend.Application.DTOs.Nomba;

namespace AjoCoreBackend.Application.Interfaces.Services
{
    public interface INombaApiClient
    {
        Task<CreateSubAccountResponse> CreateSubAccountAsync(CreateSubAccountRequest request);
        Task<CreateVirtualAccountResponse> CreateVirtualAccountAsync(CreateVirtualAccountRequest request);
        Task<BankLookupResponse> LookupBankAccountAsync(BankLookupRequest request);
        Task<BankTransferResponse> ExecuteBankTransferAsync(BankTransferRequest request);
        Task<FetchBanksResponse> FetchBanksAsync();
    }
}
