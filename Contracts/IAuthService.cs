// TransportLogistics.Api/Contracts/IAuthService.cs
using TransportLogistics.Api.DTOs;
using System.Security.Claims; // Для ClaimsPrincipal
using System.Threading.Tasks;

namespace TransportLogistics.Api.Contracts
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(UserRegistrationRequest request);
        Task<AuthResult> LoginAsync(UserLoginRequest request);
        Task<AuthResult> RefreshTokenAsync(TokenRequest request);
        Task<UserInfoDto?> GetUserInfoAsync(ClaimsPrincipal userPrincipal); // Додано для зручності, якщо ви захочете такий ендпоінт
    }
}