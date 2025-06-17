using TransportLogistics.Api.Data.Entities;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TransportLogistics.Api.Contracts
{
    public interface IJwtTokenService
    {
        Task<string> GenerateAccessToken(User user);
        Task<string> GenerateRefreshToken(User user);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}