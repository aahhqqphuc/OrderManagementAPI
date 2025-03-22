using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderManagementAPI.Controllers;
using OrderManagementAPI.DTOs;
using OrderManagementAPI.Models;
using OrderManagementAPI.Repositories;
using Xunit;

namespace OrderManagementAPI.Tests
{
    public class OrdersControllerTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<OrdersController>> _mockLogger;
        private readonly OrdersController _controller;

        public OrdersControllerTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<OrdersController>>();

            _controller = new OrdersController(
                _mockOrderRepository.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetOrders_ReturnsOkResult_WithListOfOrders()
        {
            // Arrange
            var orders = GetSampleOrders();
            var orderDtos = GetSampleOrderDtos();
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            _mockOrderRepository.Setup(repo => repo.GetAllOrdersAsync(1, 10))
                .ReturnsAsync(orders);
            _mockMapper.Setup(m => m.Map<IEnumerable<OrderDto>>(orders))
                .Returns(orderDtos);

            // Act
            var result = await _controller.GetOrders();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<OrderDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }

        [Fact]
        public async Task GetOrder_ReturnsOkResult_WithOrder()
        {
            // Arrange
            var orderId = 1;
            var order = GetSampleOrders().FirstOrDefault(o => o.Id == orderId);
            var orderDto = GetSampleOrderDtos().FirstOrDefault(o => o.Id == orderId);

            _mockOrderRepository.Setup(repo => repo.GetOrderByIdAsync(orderId))
                .ReturnsAsync(order);
            _mockMapper.Setup(m => m.Map<OrderDto>(order))
                .Returns(orderDto);

            // Act
            var result = await _controller.GetOrder(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<OrderDto>(okResult.Value);
            Assert.Equal(orderId, returnValue.Id);
        }

        [Fact]
        public async Task GetOrder_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = 99;
            _mockOrderRepository.Setup(repo => repo.GetOrderByIdAsync(orderId))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _controller.GetOrder(orderId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateOrder_ReturnsCreatedAtAction_WithNewOrder()
        {
            // Arrange
            var createOrderDto = new CreateOrderDto
            {
                CustomerName = "New Customer",
                OrderDetails = new List<CreateOrderDetailDto>
                {
                    new CreateOrderDetailDto
                    {
                        ProductName = "New Product",
                        Quantity = 1,
                        Price = 100
                    }
                }
            };

            var orderToCreate = new Order
            {
                CustomerName = "New Customer",
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        ProductName = "New Product",
                        Quantity = 1,
                        Price = 100
                    }
                }
            };

            var createdOrder = new Order
            {
                Id = 3,
                CustomerName = "New Customer",
                TotalAmount = 100,
                Status = 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                OrderDetails = new List<OrderDetail>
                {
                    new OrderDetail
                    {
                        Id = 5,
                        OrderId = 3,
                        ProductName = "New Product",
                        Quantity = 1,
                        Price = 100
                    }
                }
            };

            var orderDto = new OrderDto
            {
                Id = 3,
                CustomerName = "New Customer",
                TotalAmount = 100,
                Status = 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _mockMapper.Setup(m => m.Map<Order>(createOrderDto))
                .Returns(orderToCreate);
            _mockOrderRepository.Setup(repo => repo.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync(createdOrder);
            _mockMapper.Setup(m => m.Map<OrderDto>(createdOrder))
                .Returns(orderDto);

            // Act
            var result = await _controller.CreateOrder(createOrderDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<OrderDto>(createdAtActionResult.Value);
            Assert.Equal(3, returnValue.Id);
            Assert.Equal("New Customer", returnValue.CustomerName);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var orderId = 1;
            var updateOrderDto = new UpdateOrderDto
            {
                CustomerName = "Updated Customer",
                Status = (OrderStatus)1
            };

            var existingOrder = GetSampleOrders().FirstOrDefault(o => o.Id == orderId);

            _mockOrderRepository.Setup(repo => repo.GetOrderByIdAsync(orderId))
                .ReturnsAsync(existingOrder);
            _mockOrderRepository.Setup(repo => repo.UpdateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync(existingOrder);

            // Act
            var result = await _controller.UpdateOrder(orderId, updateOrderDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateOrder_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = 99;
            var updateOrderDto = new UpdateOrderDto
            {
                CustomerName = "Updated Customer",
                Status = (OrderStatus)1
            };

            _mockOrderRepository.Setup(repo => repo.GetOrderByIdAsync(orderId))
                .ReturnsAsync((Order)null);

            // Act
            var result = await _controller.UpdateOrder(orderId, updateOrderDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteOrder_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var orderId = 1;

            _mockOrderRepository.Setup(repo => repo.OrderExistsAsync(orderId))
                .ReturnsAsync(true);
            _mockOrderRepository.Setup(repo => repo.DeleteOrderAsync(orderId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteOrder(orderId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteOrder_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = 99;

            _mockOrderRepository.Setup(repo => repo.OrderExistsAsync(orderId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteOrder(orderId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        private List<Order> GetSampleOrders()
        {
            return new List<Order>
            {
                new Order
                {
                    Id = 1,
                    CustomerName = "John Doe",
                    TotalAmount = 150.00m,
                    Status = 0,
                    CreatedAt = DateTime.Now.AddDays(-2),
                    UpdatedAt = DateTime.Now.AddDays(-2),
                    OrderDetails = new List<OrderDetail>
                    {
                        new OrderDetail
                        {
                            Id = 1,
                            OrderId = 1,
                            ProductName = "Product A",
                            Quantity = 2,
                            Price = 50.00m
                        },
                        new OrderDetail
                        {
                            Id = 2,
                            OrderId = 1,
                            ProductName = "Product B",
                            Quantity = 1,
                            Price = 50.00m
                        }
                    }
                },
                new Order
                {
                    Id = 2,
                    CustomerName = "Jane Smith",
                    TotalAmount = 200.00m,
                    Status = (OrderStatus)1,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now,
                    OrderDetails = new List<OrderDetail>
                    {
                        new OrderDetail
                        {
                            Id = 3,
                            OrderId = 2,
                            ProductName = "Product C",
                            Quantity = 4,
                            Price = 25.00m
                        },
                        new OrderDetail
                        {
                            Id = 4,
                            OrderId = 2,
                            ProductName = "Product D",
                            Quantity = 2,
                            Price = 50.00m
                        }
                    }
                }
            };
        }

        private List<OrderDto> GetSampleOrderDtos()
        {
            return new List<OrderDto>
            {
                new OrderDto
                {
                    Id = 1,
                    CustomerName = "John Doe",
                    TotalAmount = 150.00m,
                    Status = 0,
                    CreatedAt = DateTime.Now.AddDays(-2),
                    UpdatedAt = DateTime.Now.AddDays(-2)
                },
                new OrderDto
                {
                    Id = 2,
                    CustomerName = "Jane Smith",
                    TotalAmount = 200.00m,
                    Status = (OrderStatus)1,
                    CreatedAt = DateTime.Now.AddDays(-1),
                    UpdatedAt = DateTime.Now
                }
            };
        }
    }
}