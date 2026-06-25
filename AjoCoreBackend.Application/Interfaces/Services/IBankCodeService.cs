using System.Threading.Tasks;

namespace AjoCoreBackend.Application.Interfaces.Services
{
    public interface IBankCodeService
    {
        Task<string> GetBankCodeByNameAsync(string bankName);
    }
}
