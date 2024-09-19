using CargoManagement.Api.DTOs.Auth;
using CargoManagementProject.Infrastructure.Logging;
using CargoManagement.Api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CargoManagement.Core.Services;
using CargoManagementProject.Core.Entities;
using CargoManagementProject.Infrastructure.JWT;


namespace CargoManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtTokenService _jwtTokenService;
        private readonly IUserService _userService;
        private readonly Logger _logger;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthController(JwtTokenService jwtTokenService, IUserService userService, Logger logger, IPasswordHasher<User> passwordHasher)
        {
            _jwtTokenService = jwtTokenService;
            _userService = userService;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        // Registration endpoint
        [HttpPost("register")]

        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto != null)
            {
                Console.WriteLine(registerDto.Username);
                Console.WriteLine(registerDto.Password);
            }
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid data." });

            var existingUser = await _userService.GetUserByUsernameAsync(registerDto.Username);
            if (existingUser != null)
                return Conflict(new ApiResponse { Success = false, Message = "Username already exists." });

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                Role = "User" // Default role
            };

            await _userService.RegisterUserAsync(user, registerDto.Password);
            _logger.Log($"New user registered: {user.Username}");

            return Ok(new ApiResponse { Success = true, Message = "Registration successful." });
        }

        // Login endpoint
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse), 200)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid data." });

            var user = await _userService.GetUserByUsernameAsync(loginDto.Username);
            if (user == null)
                return Unauthorized(new ApiResponse { Success = false, Message = "Invalid credentials." });

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new ApiResponse { Success = false, Message = "Invalid credentials." });

            var token = _jwtTokenService.GenerateToken(user);
            _logger.Log($"User logged in: {user.Username}");

            return Ok(new { Token = token });
        }
    }
}
