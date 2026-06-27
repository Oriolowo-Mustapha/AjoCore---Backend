using System.Threading.Tasks;
using AjoCoreBackend.Domain.Entities;

namespace AjoCoreBackend.Application.Interfaces.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(Trader user);
        string GenerateToken(string userId, string email, string role, string fullName);
        string GenerateRefreshToken();
        System.Security.Claims.ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
