
using CargoManagement.Core.Interfaces;
using CargoManagement.Core.Services;
using CargoManagementProject.core.Entities;
using CargoManagementProject.core.Interfaces;
using CargoManagementProject.Core.Enums;
using System.Threading.Tasks;

namespace CargoManagement.Api.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICargoRepository _cargoRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<OrderService> _logger; 

        public OrderService(IOrderRepository orderRepository, ICargoRepository cargoRepository, IUserRepository userRepository, ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _cargoRepository = cargoRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync()
        {
            return await _orderRepository.GetAllOrdersAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _orderRepository.GetOrdersByUserIdAsync(userId);
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            return await _orderRepository.GetOrderByIdAsync(id);
        }

        public async Task CreateOrderAsync(Order order)
        {
            // Validate User and Cargo
            var user = await _userRepository.GetUserByIdAsync(order.UserId);
            var cargo = await _cargoRepository.GetCargoByIdAsync(order.CargoId);

            if (user == null)
                throw new Exception("User not found.");

            if (cargo == null)
                throw new Exception("Cargo not found.");

            order.Status = OrderStatus.Pending;
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.AddOrderAsync(order);
        }

        public async Task UpdateOrderAsync(Order order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateOrderAsync(order);
        }

        public async Task DeleteOrderAsync(int id)
        {
            await _orderRepository.DeleteOrderAsync(id);
        }
    }
}
