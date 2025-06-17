    using Microsoft.AspNetCore.Mvc;
    using TransportLogistics.Api.Contracts;
    using TransportLogistics.Api.DTOs; // Ця директива потрібна для UserRegistrationRequest, UserLoginRequest, AuthResult, TokenRequest
    using System.Threading.Tasks;
    // using Microsoft.AspNetCore.Identity; // Закоментовано, бо UserManager більше не використовується тут напряму
    // using TransportLogistics.Api.Data.Entities; // Закоментовано, бо User Entity не використовується тут напряму
    using System.Linq; // Залишено, якщо потрібно для .Select

    namespace TransportLogistics.Api.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class AuthController : ControllerBase
        {
            private readonly IAuthService _authService;
            // UserManager<User> та RoleManager більше не потрібні в цьому контролері,
            // оскільки методи управління ролями/паролями були видалені.
            // Якщо ви захочете їх повернути, то UserManager потрібно буде додати назад.

            public AuthController(IAuthService authService) // Прибрано UserManager та RoleManager з конструктора
            {
                _authService = authService;
            }

            [HttpPost("register")]
            public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request) // Використовуємо UserRegistrationRequest
            {
                var response = await _authService.RegisterAsync(request);
                if (!response.Success)
                {
                    return BadRequest(response.Errors);
                }
                return Ok(response);
            }

            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] UserLoginRequest request) // Використовуємо UserLoginRequest
            {
                var response = await _authService.LoginAsync(request);
                if (!response.Success)
                {
                    return Unauthorized(response.Errors);
                }
                return Ok(response);
            }

            [HttpPost("refresh-token")]
            public async Task<IActionResult> RefreshToken([FromBody] TokenRequest request)
            {
                var response = await _authService.RefreshTokenAsync(request);
                if (!response.Success)
                {
                    return Unauthorized(response.Errors);
                }
                return Ok(response);
            }

            // !!! ВАЖЛИВО: НАСТУПНІ МЕТОДИ БУЛИ ВИДАЛЕНІ !!!
            // Оскільки DTO для них (AssignRoleRequest, RevokeRoleRequest, UpdatePasswordRequest,
            // ForgotPasswordRequest, ResetPasswordRequest) не існують у вашому проекті.
            // Якщо ви хочете додати цю функціональність, вам потрібно буде:
            // 1. Створити відповідні DTO-файли у TransportLogistics.Api/DTOs.
            // 2. Реалізувати відповідну логіку (можливо, в AuthService або окремому сервісі).
            // 3. Розкоментувати ці методи тут або створити нові.
            /*
            [HttpPost("assign-role")]
            public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
            {
                throw new NotImplementedException("AssignRole endpoint requires corresponding DTO and AuthService implementation.");
            }

            [HttpPost("revoke-role")]
            public async Task<IActionResult> RevokeRole([FromBody] RevokeRoleRequest request)
            {
                throw new NotImplementedException("RevokeRole endpoint requires corresponding DTO and AuthService implementation.");
            }

            [HttpPost("update-password")]
            public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest request)
            {
                throw new NotImplementedException("UpdatePassword endpoint requires corresponding DTO and AuthService implementation.");
            }

            [HttpPost("forgot-password")]
            public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
            {
                throw new NotImplementedException("ForgotPassword endpoint requires corresponding DTO and AuthService implementation.");
            }

            [HttpPost("reset-password")]
            public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
            {
                throw new NotImplementedException("ResetPassword endpoint requires corresponding DTO and AuthService implementation.");
            }
            */
        }
    }
    