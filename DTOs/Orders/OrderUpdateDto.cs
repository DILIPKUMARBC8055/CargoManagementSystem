using System.ComponentModel.DataAnnotations;
using CargoManagementProject.Core.Enums;

namespace CargoManagement.Api.DTOs.Orders
{
    public class OrderUpdateDto
    {
        [Required]
        public OrderStatus Status { get; set; }

        
    }
}
