
using CargoManagement.Core.Interfaces;
using CargoManagement.Core.Services;
using CargoManagementProject.core.Interfaces;
using CargoManagementProject.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using System.Threading.Tasks;

namespace CargoManagement.Api.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ICacheService _cacheService;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, ICacheService cacheService, ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            _logger.LogInformation("started to fetch all users");

            string cacheKey = "getAllUsers";
            IEnumerable<User> users = await _cacheService.GetAsync<IEnumerable<User>>(cacheKey);

            if (users == null)
            {
                users = await _userRepository.GetAllUsersAsync();
                await _cacheService.SetAsync<IEnumerable<User>>(cacheKey, users, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(5));
                _logger.LogInformation("fetch the users from database");

            }
            else
            {
                _logger.LogInformation("fetch the users from cache");

            }

            return users;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            string cacheKey = "getAllUsers";
            IEnumerable<User> users = await _cacheService.GetAsync<IEnumerable<User>>(cacheKey);
            if (users != null)
            {
                User userFound = users.FirstOrDefault(user => user.Id == id);
                if (userFound != null)
                {
                    return userFound;
                }

            }
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _userRepository.GetUserByUsernameAsync(username);
        }

        public async Task RegisterUserAsync(User user, string password)
        {
            string cacheKey = "getAllUsers";
            await _cacheService.RemoveAsync(cacheKey);
            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _userRepository.AddUserAsync(user);
        }

        public async Task UpdateUserAsync(User user)
        {
            string cacheKey = "getAllUsers";
            await _cacheService.RemoveAsync(cacheKey);
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task DeleteUserAsync(int id)
        {
            string cacheKey = "getAllUsers";
            await _cacheService.RemoveAsync(cacheKey);
            await _userRepository.DeleteUserAsync(id);
        }


    }
}
