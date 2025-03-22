using OrderManagementAPI.Models;

namespace OrderManagementAPI.Repositories
{
    public interface IOrderDetailRepository
    {
        Task<IEnumerable<OrderDetail>> GetOrderDetailsByOrderIdAsync(int orderId);

        Task<OrderDetail> GetOrderDetailByIdAsync(int id);

        Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail);

        Task<bool> DeleteOrderDetailAsync(int id);

        Task<bool> OrderDetailExistsAsync(int id);
    }
}