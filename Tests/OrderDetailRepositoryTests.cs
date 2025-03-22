using Microsoft.EntityFrameworkCore;
using OrderManagementAPI.Data;
using OrderManagementAPI.Models;
using OrderManagementAPI.Repositories;
using Xunit;

namespace OrderManagementAPI.Tests
{
    public class OrderDetailRepositoryTests
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
                for (int i = 1; i <= 5; i++)
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
        public async Task GetOrderDetailsByOrderIdAsync_ReturnsOrderDetails()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderDetailRepository = new OrderDetailRepository(dbContext);
            var orderId = 1;

            // Act
            var result = await orderDetailRepository.GetOrderDetailsByOrderIdAsync(orderId);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.True(result.All(od => od.OrderId == orderId));
        }

        [Fact]
        public async Task GetOrderDetailByIdAsync_ReturnsOrderDetail_WhenOrderDetailExists()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderDetailRepository = new OrderDetailRepository(dbContext);
            var orderDetailId = 1;

            // Act
            var result = await orderDetailRepository.GetOrderDetailByIdAsync(orderDetailId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderDetailId, result.Id);
            Assert.Equal("Product 1A", result.ProductName);
        }

        [Fact]
        public async Task GetOrderDetailByIdAsync_ReturnsNull_WhenOrderDetailDoesNotExist()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderDetailRepository = new OrderDetailRepository(dbContext);
            var orderDetailId = 99;

            // Act
            var result = await orderDetailRepository.GetOrderDetailByIdAsync(orderDetailId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateOrderDetailAsync_CreatesOrderDetail_AndReturnsCreatedOrderDetail()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderDetailRepository = new OrderDetailRepository(dbContext);
            var newOrderDetail = new OrderDetail
            {
                OrderId = 1,
                ProductName = "New Product",
                Quantity = 1,
                Price = 100.00m
            };

            // Act
            var result = await orderDetailRepository.CreateOrderDetailAsync(newOrderDetail);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(0, result.Id);
            Assert.Equal("New Product", result.ProductName);
            Assert.Equal(1, result.OrderId);
        }

        [Fact]
        public async Task DeleteOrderDetailAsync_DeletesOrderDetail_AndReturnsTrue()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderDetailRepository = new OrderDetailRepository(dbContext);
            var orderDetailId = 1;

            // Act
            var result = await orderDetailRepository.DeleteOrderDetailAsync(orderDetailId);
            var deletedOrderDetail = await orderDetailRepository.GetOrderDetailByIdAsync(orderDetailId);

            // Assert
            Assert.True(result);
            Assert.Null(deletedOrderDetail);
        }

        [Fact]
        public async Task OrderDetailExistsAsync_ReturnsTrue_WhenOrderDetailExists()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderDetailRepository = new OrderDetailRepository(dbContext);
            var orderDetailId = 1;
            // Act
            var result = await orderDetailRepository.OrderDetailExistsAsync(orderDetailId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task OrderDetailExistsAsync_ReturnsFalse_WhenOrderDetailDoesNotExist()
        {
            // Arrange
            var dbContext = await GetDatabaseContext();
            var orderDetailRepository = new OrderDetailRepository(dbContext);
            var orderDetailId = 99;

            // Act
            var result = await orderDetailRepository.OrderDetailExistsAsync(orderDetailId);

            // Assert
            Assert.False(result);
        }
    }
}