using OrderManagementAPI.Models;

namespace OrderManagementAPI.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync(int page = 1, int pageSize = 10);

        Task<Order> GetOrderByIdAsync(int id);

        Task<Order> CreateOrderAsync(Order order);

        Task<Order> UpdateOrderAsync(Order order);

        Task<bool> DeleteOrderAsync(int id);

        Task<int> GetTotalOrderCountAsync();

        Task<bool> OrderExistsAsync(int id);
    }
}