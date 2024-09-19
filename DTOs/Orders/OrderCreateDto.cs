using System.ComponentModel.DataAnnotations;

namespace CargoManagement.Api.DTOs.Orders
{
    public class OrderCreateDto
    {
        [Required]
        public int CargoId { get; set; }

        
    }
}
