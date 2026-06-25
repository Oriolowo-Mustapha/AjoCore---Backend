using System.Threading.Tasks;
using AjoCoreBackend.Domain.Entities;

namespace AjoCoreBackend.Application.Interfaces.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(Trader user);
    }
}
