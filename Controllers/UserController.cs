using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CargoManagement.Api.DTOs.Users;
using CargoManagement.Api.Responses;
using CargoManagement.Core.Services;
using CargoManagementProject.Infrastructure.Logging;
using CargoManagementProject.Core.Entities;


namespace CargoManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   
    //[Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        private readonly Logger _logger;

        public UserController(IUserService userService, Logger logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> changeUserRole(int id, [FromBody] String role)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return BadRequest(new ApiResponse()
                {
                    Success = false,
                    Message = "user Id not found"
                });

            }
            user.Role = role;
            await _userService.UpdateUserAsync(user);

            return Ok(new ApiResponse<User>() { Success = true, Data = user });

        }

        // GET: api/User
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            
            var users = await _userService.GetAllUsersAsync();
            var userDtos = users.Select(u => new UserResponseDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            });

            return Ok(new ApiResponse<IEnumerable<UserResponseDto>>
            {
                Success = true,
                Data = userDtos
            });
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new ApiResponse { Success = false, Message = "User not found." });

            var userDto = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };

            return Ok(new ApiResponse<UserResponseDto>
            {
                Success = true,
                Data = userDto
            });
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid data." });

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new ApiResponse { Success = false, Message = "User not found." });

            user.Username = userDto.Username;
            user.Email = userDto.Email;

            await _userService.UpdateUserAsync(user);
            _logger.Log($"User updated: {user.Username}");

            return Ok(new ApiResponse { Success = true, Message = "User updated successfully." });
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new ApiResponse { Success = false, Message = "User not found." });

            await _userService.DeleteUserAsync(id);
            _logger.Log($"User deleted: {user.Username}");

            return Ok(new ApiResponse { Success = true, Message = "User deleted successfully." });
        }
    }
}
