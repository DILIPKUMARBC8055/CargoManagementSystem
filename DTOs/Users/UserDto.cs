using System.ComponentModel.DataAnnotations;

namespace CargoManagement.Api.DTOs.Users
{
    public class UserDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
