// TransportLogistics.Api/Services/AuthService.cs
using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.Data.Entities;
using TransportLogistics.Api.DTOs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Security.Claims; // Для ClaimsPrincipal
using System.Threading.Tasks;

namespace TransportLogistics.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResult> RegisterAsync(UserRegistrationRequest request)
        {
            var user = new User
            {
                Email = request.Email,
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return new AuthResult { Success = false, Errors = result.Errors.Select(e => e.Description).ToList() };
            }

            // Використовуємо await GenerateAccessToken та GenerateRefreshToken
            var token = await _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = await _jwtTokenService.GenerateRefreshToken(user); // Передаємо user

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            return new AuthResult { Success = true, Token = token, RefreshToken = refreshToken };
        }

        public async Task<AuthResult> LoginAsync(UserLoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return new AuthResult { Success = false, Errors = new List<string> { "Invalid credentials" } };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return new AuthResult { Success = false, Errors = new List<string> { "Invalid credentials" } };
            }

            // Використовуємо await GenerateAccessToken та GenerateRefreshToken
            var token = await _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = await _jwtTokenService.GenerateRefreshToken(user); // Передаємо user

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userManager.UpdateAsync(user);

            return new AuthResult { Success = true, Token = token, RefreshToken = refreshToken };
        }

        public async Task<AuthResult> RefreshTokenAsync(TokenRequest request)
        {
            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.Token);
            if (principal == null)
            {
                return new AuthResult { Success = false, Errors = new List<string> { "Invalid token" } };
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return new AuthResult { Success = false, Errors = new List<string> { "Invalid token" } };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return new AuthResult { Success = false, Errors = new List<string> { "Invalid refresh token or token expired" } };
            }

            // Використовуємо await GenerateAccessToken та GenerateRefreshToken
            var newToken = await _jwtTokenService.GenerateAccessToken(user);
            var newRefreshToken = await _jwtTokenService.GenerateRefreshToken(user); // Передаємо user

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthResult { Success = true, Token = newToken, RefreshToken = newRefreshToken };
        }

        public async Task<UserInfoDto?> GetUserInfoAsync(ClaimsPrincipal userPrincipal)
        {
            var userIdClaim = userPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return null;
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return null;
            }

            return new UserInfoDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty
            };
        }
    }
}
