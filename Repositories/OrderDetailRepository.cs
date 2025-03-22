using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Data;
using OrderManagementAPI.Models;

namespace OrderManagementAPI.Repositories
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderDetailRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderDetail>> GetOrderDetailsByOrderIdAsync(int orderId)
        {
            return await _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .ToListAsync();
        }

        public async Task<OrderDetail> GetOrderDetailByIdAsync(int id)
        {
            return await _context.OrderDetails.FindAsync(id);
        }

        public async Task<OrderDetail> CreateOrderDetailAsync(OrderDetail orderDetail)
        {
            _context.OrderDetails.Add(orderDetail);

            // Update the order's total amount
            var order = await _context.Orders.FindAsync(orderDetail.OrderId);
            if (order != null)
            {
                order.TotalAmount += orderDetail.Price * orderDetail.Quantity;
                order.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return orderDetail;
        }

        public async Task<bool> DeleteOrderDetailAsync(int id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null)
                return false;

            // Update the order's total amount
            var order = await _context.Orders.FindAsync(orderDetail.OrderId);
            if (order != null)
            {
                order.TotalAmount -= orderDetail.Price * orderDetail.Quantity;
                order.UpdatedAt = DateTime.UtcNow;
            }

            _context.OrderDetails.Remove(orderDetail);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> OrderDetailExistsAsync(int id)
        {
            return await _context.OrderDetails.AnyAsync(od => od.Id == id);
        }
    }
}