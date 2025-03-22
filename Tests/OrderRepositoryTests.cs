using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Data;
using OrderManagementAPI.Models;
using OrderManagementAPI.Repositories;
using Xunit;

namespace OrderManagementAPI.Tests
{
    public class OrderRepositoryTests
    {
        private async Task<ApplicationDbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();

            if (await databaseContext.Orders.CountAsync() <= 0)
            {
                for (int i = 1; i <= 10; i++)
                {
                    databaseContext.Orders.Add(new Order()
                    {
                        Id = i,
                        CustomerName = $"Customer {i}",
                        TotalAmount = i * 100.00m,
                        Status = (OrderStatus)(i % 3),
                        CreatedAt = DateTime.Now.AddDays(-i),
                        UpdatedAt = DateTime.Now.AddDays(-i + 1),
                        OrderDetails = new List<OrderDetail>
                        {
                            new OrderDetail
                            {
                                Id = i * 2 - 1,
                                OrderId = i,
                                ProductName = $"Product {i}A",
                                Quantity = i,
                                Price = 50.00m
                            },
                            new OrderDetail
                            {
                                Id = i * 2,
                                OrderId = i,
                                ProductName = $"Product {i}B",
                                Quantity = i,
                                Price = 50.00m
                            }
                        }
                    });
                    await databaseContext.SaveChangesAsync();
                }
            }
            return databaseContext;
        }

        [Fact]
        public async Task GetAllOrdersAsync_ReturnsAllOrders()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderRepository = new OrderRepository(dbContext);

            // Act
            var result = await orderRepository.GetAllOrdersAsync();

            // Assert
            Assert.Equal(10, result.Count());
        }

        [Fact]
        public async Task GetAllOrdersAsync_WithPagination_ReturnsCorrectOrders()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderRepository = new OrderRepository(dbContext);

            // Act
            var result = await orderRepository.GetAllOrdersAsync(2, 3);

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal(4, result.First().Id);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ReturnsOrder_WhenOrderExists()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderRepository = new OrderRepository(dbContext);
            var orderId = 1;

            // Act
            var result = await orderRepository.GetOrderByIdAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
            Assert.Equal("Customer 1", result.CustomerName);
        }

        [Fact]
        public async Task GetOrderByIdAsync_ReturnsNull_WhenOrderDoesNotExist()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderRepository = new OrderRepository(dbContext);
            var orderId = 99;

            // Act
            var result = await orderRepository.GetOrderByIdAsync(orderId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateOrderAsync_CreatesOrder_AndReturnsCreatedOrder()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderRepository = new OrderRepository(dbContext);
            var newOrder = new Order
            {
                CustomerName = "New Customer",
                Status = 0,
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        ProductName = "New Product",
                        Quantity = 1,
                        Price = 100.00m
                    }
                }
            };

            // Act
            var result = await orderRepository.CreateOrderAsync(newOrder);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(0, result.Id);
            Assert.Equal("New Customer", result.CustomerName);
            Assert.Equal(100.00m, result.TotalAmount);
            Assert.Equal(1, result.OrderDetails.Count);
        }

        [Fact]
        public async Task UpdateOrderAsync_UpdatesOrder_AndReturnsTrue()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderRepository = new OrderRepository(dbContext);
            var orderId = 1;
            var order = await orderRepository.GetOrderByIdAsync(orderId);
            order.CustomerName = "Updated Customer";
            order.Status = (OrderStatus)1;

            // Act
            var result = await orderRepository.UpdateOrderAsync(order);
            var updatedOrder = await orderRepository.GetOrderByIdAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Customer", updatedOrder.CustomerName);
            Assert.Equal((OrderStatus)1, updatedOrder.Status);
        }

        [Fact]
        public async Task DeleteOrderAsync_DeletesOrder_AndReturnsTrue()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderRepository = new OrderRepository(dbContext);
            var orderId = 1;

            // Act
            var result = await orderRepository.DeleteOrderAsync(orderId);
            var deletedOrder = await orderRepository.GetOrderByIdAsync(orderId);

            // Assert
            Assert.True(result);
            Assert.Null(deletedOrder);
        }

        [Fact]
        public async Task OrderExistsAsync_ReturnsTrue_WhenOrderExists()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderRepository = new OrderRepository(dbContext);
            var orderId = 1;

            // Act
            var result = await orderRepository.OrderExistsAsync(orderId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task OrderExistsAsync_ReturnsFalse_WhenOrderDoesNotExist()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderRepository = new OrderRepository(dbContext);
            var orderId = 99;

            // Act
            var result = await orderRepository.OrderExistsAsync(orderId);

            // Assert
            Assert.False(result);
        }
    }
}