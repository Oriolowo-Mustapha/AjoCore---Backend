using AjoCoreBackend.Application.DTOs.Nomba;
using System.Threading.Tasks;

namespace AjoCoreBackend.Application.Interfaces.Services
{
    public interface IBankCodeService
    {
        Task<string> GetBankCodeByNameAsync(string bankName);
        Task<IEnumerable<BankDto>> GetAllBanksAsync();
    }
}
