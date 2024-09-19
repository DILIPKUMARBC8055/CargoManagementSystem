using CargoManagement.Api.DTOs.Users;
using CargoManagement.DTOs;
using CargoManagementProject.Core.Enums;

namespace CargoManagement.Api.DTOs.Orders
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public UserDto User { get; set; }
        public CargoDto Cargo { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
