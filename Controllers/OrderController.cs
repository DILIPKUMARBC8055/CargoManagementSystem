using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CargoManagement.Api.DTOs.Orders;
using CargoManagement.Api.Responses;
using CargoManagement.Core.Services;

using CargoManagement.Api.DTOs.Users;
using CargoManagement.DTOs;

using CargoManagementProject.core.Entities;
using CargoManagementProject.Infrastructure.Logging;

namespace CargoManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly Logger _logger;

        public OrderController(IOrderService orderService, IUserService userService, Logger logger)
        {
            _orderService = orderService;
            _userService = userService;
            _logger = logger;
        }

        // GET: api/Order
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            var orderDtos = orders.Select(o => new OrderResponseDto
            {
                Id = o.Id,
                User = new UserDto { Username = o.User.Username, Email = o.User.Email },
                Cargo = new CargoDto { Name = o.Cargo.Name, Weight = o.Cargo.Weight, Destination = o.Cargo.Destination },
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            });

            return Ok(new ApiResponse<IEnumerable<OrderResponseDto>>
            {
                Success = true,
                Data = orderDtos
            });
        }

        // GET: api/Order/MyOrders
        [HttpGet("MyOrders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var username = User.Identity.Name;
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
                return Unauthorized(new ApiResponse { Success = false, Message = "User not found." });

            var orders = await _orderService.GetOrdersByUserIdAsync(user.Id);
            var orderDtos = orders.Select(o => new OrderResponseDto
            {
                Id = o.Id,
                User = new UserDto { Username = o.User.Username, Email = o.User.Email },
                Cargo = new CargoDto { Name = o.Cargo.Name, Weight = o.Cargo.Weight, Destination = o.Cargo.Destination },
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                UpdatedAt = o.UpdatedAt
            });

            return Ok(new ApiResponse<IEnumerable<OrderResponseDto>>
            {
                Success = true,
                Data = orderDtos
            });
        }

        // GET: api/Order/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new ApiResponse { Success = false, Message = "Order not found." });

            // Ensure that only Admins or the owner can view the order
            var username = User.Identity.Name;
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user.Id != order.UserId && user.Role != "Admin")
                return Forbid();

            var orderDto = new OrderResponseDto
            {
                Id = order.Id,
                User = new UserDto { Username = order.User.Username, Email = order.User.Email },
                Cargo = new CargoDto { Name = order.Cargo.Name, Weight = order.Cargo.Weight, Destination = order.Cargo.Destination },
                Status = order.Status,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };

            return Ok(new ApiResponse<OrderResponseDto>
            {
                Success = true,
                Data = orderDto
            });
        }

        // POST: api/Order
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto orderCreateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid data." });

            var username = User.Identity.Name;
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null)
                return Unauthorized(new ApiResponse { Success = false, Message = "User not found." });

            var order = new Order
            {
                UserId = user.Id,
                CargoId = orderCreateDto.CargoId
            };

            try
            {
                await _orderService.CreateOrderAsync(order);
                _logger.Log($"Order created: {order.Id} by {user.Username}");
                return Ok(new ApiResponse { Success = true, Message = "Order created successfully." });
            }
            catch (Exception ex)
            {
                _logger.Log($"Error creating order: {ex.Message}");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while creating the order." });
            }
        }

        // PUT: api/Order/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrderUpdateDto orderUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse { Success = false, Message = "Invalid data." });

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new ApiResponse { Success = false, Message = "Order not found." });

            order.Status = orderUpdateDto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _orderService.UpdateOrderAsync(order);
                _logger.Log($"Order updated: {order.Id} to status {order.Status}");
                return Ok(new ApiResponse { Success = true, Message = "Order updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.Log($"Error updating order: {ex.Message}");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while updating the order." });
            }
        }

        // DELETE: api/Order/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound(new ApiResponse { Success = false, Message = "Order not found." });

            try
            {
                await _orderService.DeleteOrderAsync(id);
                _logger.Log($"Order deleted: {order.Id}");
                return Ok(new ApiResponse { Success = true, Message = "Order deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.Log($"Error deleting order: {ex.Message}");
                return StatusCode(500, new ApiResponse { Success = false, Message = "An error occurred while deleting the order." });
            }
        }
    }
}
