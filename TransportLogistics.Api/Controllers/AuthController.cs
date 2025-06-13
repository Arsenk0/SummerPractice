using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TransportLogistics.Api.Contracts;
using TransportLogistics.Api.Data.Entities;
using TransportLogistics.Api.DTOs;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq; // Додано для SelectMany
using System.Security.Claims; // !!! ДОДАНО ЦЕЙ РЯДОК !!!

namespace TransportLogistics.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtTokenService jwtTokenService,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(),
                    Success = false
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest(new AuthResult
                {
                    Errors = new List<string> { "Email already exists" },
                    Success = false
                });
            }

            var newUser = new User
            {
                Email = model.Email,
                UserName = model.Email, // Зазвичай email використовується як UserName для входу
                FirstName = model.FirstName, // Це поле тепер є в User.cs
                LastName = model.LastName    // Це поле тепер є в User.cs
            };

            var isCreated = await _userManager.CreateAsync(newUser, model.Password);

            if (!isCreated.Succeeded)
            {
                var errors = new List<string>();
                foreach (var error in isCreated.Errors)
                {
                    errors.Add(error.Description);
                }
                return BadRequest(new AuthResult
                {
                    Errors = errors,
                    Success = false
                });
            }

            // Опціонально: автоматичний вхід після реєстрації
            await _signInManager.SignInAsync(newUser, isPersistent: false); // !!! ЗМІНЕНО ЦЕЙ РЯДОК !!!

            // Генерація токенів
            var accessToken = await _jwtTokenService.GenerateAccessToken(newUser);
            var refreshToken = await _jwtTokenService.GenerateRefreshToken(newUser);

            _logger.LogInformation($"User {newUser.Email} registered successfully.");

            return Ok(new AuthResult
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Success = true
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthResult
                {
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(),
                    Success = false
                });
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser == null)
            {
                return BadRequest(new AuthResult
                {
                    Errors = new List<string> { "Invalid authentication credentials" },
                    Success = false
                });
            }

            var isCorrectPassword = await _userManager.CheckPasswordAsync(existingUser, model.Password);

            if (!isCorrectPassword)
            {
                return BadRequest(new AuthResult
                {
                    Errors = new List<string> { "Invalid authentication credentials" },
                    Success = false
                });
            }

            // Генерація токенів
            var accessToken = await _jwtTokenService.GenerateAccessToken(existingUser);
            var refreshToken = await _jwtTokenService.GenerateRefreshToken(existingUser);

            _logger.LogInformation($"User {existingUser.Email} logged in successfully.");

            return Ok(new AuthResult
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                Success = true
            });
        }

        // Додаємо метод для оновлення токена (refresh token)
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest request)
        {
            var authResult = new AuthResult { Success = false };

            if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.RefreshToken))
            {
                authResult.Errors.Add("Invalid client request");
                return BadRequest(authResult);
            }

            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.Token);
            // Поле ClaimTypes.NameIdentifier тепер розпізнається завдяки using System.Security.Claims;
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                authResult.Errors.Add("Invalid token claims");
                return BadRequest(authResult);
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                authResult.Errors.Add("Invalid refresh token");
                return BadRequest(authResult);
            }

            var newAccessToken = await _jwtTokenService.GenerateAccessToken(user);
            var newRefreshToken = await _jwtTokenService.GenerateRefreshToken(user);

            return Ok(new AuthResult
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                Success = true
            });
        }
    }
}