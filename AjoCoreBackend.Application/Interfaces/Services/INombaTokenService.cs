using System.Threading.Tasks;

namespace AjoCoreBackend.Application.Interfaces.Services
{
    public interface INombaTokenService
    {
        Task<string> GetAccessTokenAsync();
    }
}
