using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Data;
using OrderManagementAPI.Models;
using System.Data;

namespace OrderManagementAPI.Repositories
{
    public class OrderRepositorySP : IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _connectionString;

        public OrderRepositorySP(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Order>> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 10)
        {
            var orders = new List<Order>();

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("GetOrdersPaginated", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@PageNumber", pageNumber);
                    command.Parameters.AddWithValue("@PageSize", pageSize);

                    var totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(totalCountParam);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            orders.Add(new Order
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                Status = reader.GetFieldValue<OrderStatus>(reader.GetOrdinal("Status")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                            });
                        }
                    }
                }
            }

            return orders;
        }

        public async Task<Order> GetOrderByIdAsync(int id)
        {
            Order order = null;

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("GetOrderWithDetails", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@OrderId", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // First result set contains the order
                        if (await reader.ReadAsync())
                        {
                            order = new Order
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                                TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                                Status = reader.GetFieldValue<OrderStatus>(reader.GetOrdinal("Status")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                                OrderDetails = new List<OrderDetail>()
                            };
                        }

                        // Move to the next result set (order details)
                        if (order != null && await reader.NextResultAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                order.OrderDetails.Add(new OrderDetail
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    OrderId = reader.GetInt32(reader.GetOrdinal("OrderId")),
                                    ProductName = reader.GetString(reader.GetOrdinal("ProductName")),
                                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                                    Price = reader.GetDecimal(reader.GetOrdinal("Price"))
                                });
                            }
                        }
                    }
                }
            }

            return order;
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            // Use a DataTable to pass the order details
            var orderDetailsTable = new DataTable();
            orderDetailsTable.Columns.Add("ProductName", typeof(string));
            orderDetailsTable.Columns.Add("Quantity", typeof(int));
            orderDetailsTable.Columns.Add("Price", typeof(decimal));

            foreach (var detail in order.OrderDetails)
            {
                orderDetailsTable.Rows.Add(detail.ProductName, detail.Quantity, detail.Price);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("CreateOrder", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                    command.Parameters.AddWithValue("@Status", order.Status);

                    var orderDetailsParam = new SqlParameter("@OrderDetails", SqlDbType.Structured)
                    {
                        TypeName = "OrderDetailsTableType",
                        Value = orderDetailsTable
                    };
                    command.Parameters.Add(orderDetailsParam);

                    var newOrderIdParam = new SqlParameter("@NewOrderId", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(newOrderIdParam);

                    await command.ExecuteNonQueryAsync();

                    // Get the new order ID
                    int newOrderId = (int)newOrderIdParam.Value;

                    // Get the created order with details
                    return await GetOrderByIdAsync(newOrderId);
                }
            }
        }

        public async Task<Order> UpdateOrderAsync(Order order)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("UpdateOrder", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@OrderId", order.Id);
                    command.Parameters.AddWithValue("@CustomerName", order.CustomerName);
                    command.Parameters.AddWithValue("@Status", order.Status);

                    await command.ExecuteNonQueryAsync();

                    // Get the updated order with details
                    return await GetOrderByIdAsync(order.Id);
                }
            }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand("DeleteOrder", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@OrderId", id);

                    int affectedRows = await command.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
        }

        public async Task<int> GetTotalOrderCountAsync()
        {
            return await _context.Orders.CountAsync();
        }

        public async Task<bool> OrderExistsAsync(int id)
        {
            return await _context.Orders.AnyAsync(o => o.Id == id);
        }
    }
}