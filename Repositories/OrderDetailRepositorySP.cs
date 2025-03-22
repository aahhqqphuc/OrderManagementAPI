using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Data;
using OrderManagementAPI.Models;
using System.Data;

namespace OrderManagementAPI.Repositories
{
    public class OrderDetailRepositorySP : IOrderDetailRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public OrderDetailRepositorySP(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
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
            int newDetailId;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("AddOrderDetail", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@OrderId", orderDetail.OrderId);
                    command.Parameters.AddWithValue("@ProductName", orderDetail.ProductName);
                    command.Parameters.AddWithValue("@Quantity", orderDetail.Quantity);
                    command.Parameters.AddWithValue("@Price", orderDetail.Price);

                    var newDetailIdParam = new SqlParameter("@NewDetailId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(newDetailIdParam);

                    await command.ExecuteNonQueryAsync();

                    newDetailId = (int)newDetailIdParam.Value;
                }
            }

            return await _context.OrderDetails.FindAsync(newDetailId);
        }

        public async Task<bool> DeleteOrderDetailAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("DeleteOrderDetail", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@OrderDetailId", id);

                    int affectedRows = await command.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
        }

        public async Task<bool> OrderDetailExistsAsync(int id)
        {
            return await _context.OrderDetails.AnyAsync(od => od.Id == id);
        }
    }
}