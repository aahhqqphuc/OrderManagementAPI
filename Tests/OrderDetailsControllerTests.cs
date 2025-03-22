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
    public class OrderDetailsControllerTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IOrderDetailRepository> _mockOrderDetailRepository;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILogger<OrderDetailsController>> _mockLogger;
        private readonly OrderDetailsController _controller;

        public OrderDetailsControllerTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockOrderDetailRepository = new Mock<IOrderDetailRepository>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILogger<OrderDetailsController>>();

            _controller = new OrderDetailsController(
                _mockOrderDetailRepository.Object,
                _mockOrderRepository.Object,
                _mockMapper.Object,
                _mockLogger.Object);
        }

        [Fact]
        public async Task GetOrderDetails_ReturnsOkResult_WithOrderDetails()
        {
            // Arrange
            var orderId = 1;
            var orderDetails = GetSampleOrderDetails().Where(od => od.OrderId == orderId).ToList();
            var orderDetailDtos = GetSampleOrderDetailDtos().Where(od => od.OrderId == orderId).ToList();

            _mockOrderRepository.Setup(repo => repo.OrderExistsAsync(orderId))
                .ReturnsAsync(true);
            _mockOrderDetailRepository.Setup(repo => repo.GetOrderDetailsByOrderIdAsync(orderId))
                .ReturnsAsync(orderDetails);
            _mockMapper.Setup(m => m.Map<IEnumerable<OrderDetailDto>>(orderDetails))
                .Returns(orderDetailDtos);

            // Act
            var result = await _controller.GetOrderDetails(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsAssignableFrom<IEnumerable<OrderDetailDto>>(okResult.Value);
            Assert.Equal(2, returnValue.Count());
        }

        [Fact]
        public async Task GetOrderDetails_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = 99;

            _mockOrderRepository.Setup(repo => repo.OrderExistsAsync(orderId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.GetOrderDetails(orderId);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task AddOrderDetail_ReturnsCreatedAtAction_WithNewOrderDetail()
        {
            // Arrange
            var orderId = 1;
            var createOrderDetailDto = new CreateOrderDetailDto
            {
                ProductName = "New Product",
                Quantity = 1,
                Price = 100
            };

            var orderDetailToCreate = new OrderDetail
            {
                OrderId = orderId,
                ProductName = "New Product",
                Quantity = 1,
                Price = 100
            };

            var createdOrderDetail = new OrderDetail
            {
                Id = 5,
                OrderId = orderId,
                ProductName = "New Product",
                Quantity = 1,
                Price = 100
            };

            var orderDetailDto = new OrderDetailDto
            {
                Id = 5,
                OrderId = orderId,
                ProductName = "New Product",
                Quantity = 1,
                Price = 100
            };

            _mockOrderRepository.Setup(repo => repo.OrderExistsAsync(orderId))
                .ReturnsAsync(true);
            _mockMapper.Setup(m => m.Map<OrderDetail>(createOrderDetailDto))
                .Returns(orderDetailToCreate);
            _mockOrderDetailRepository.Setup(repo => repo.CreateOrderDetailAsync(It.IsAny<OrderDetail>()))
                .ReturnsAsync(createdOrderDetail);
            _mockMapper.Setup(m => m.Map<OrderDetailDto>(createdOrderDetail))
                .Returns(orderDetailDto);

            // Act
            var result = await _controller.CreateOrderDetail(orderId, createOrderDetailDto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<OrderDetailDto>(createdAtActionResult.Value);
            Assert.Equal(5, returnValue.Id);
            Assert.Equal("New Product", returnValue.ProductName);
            Assert.Equal(orderId, returnValue.OrderId);
        }

        [Fact]
        public async Task AddOrderDetail_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = 99;
            var createOrderDetailDto = new CreateOrderDetailDto
            {
                ProductName = "New Product",
                Quantity = 1,
                Price = 100
            };

            _mockOrderRepository.Setup(repo => repo.OrderExistsAsync(orderId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CreateOrderDetail(orderId, createOrderDetailDto);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task DeleteOrderDetail_ReturnsNoContent_WhenSuccessful()
        {
            // Arrange
            var orderDetailId = 1;

            _mockOrderDetailRepository.Setup(repo => repo.OrderDetailExistsAsync(orderDetailId))
                .ReturnsAsync(true);
            _mockOrderDetailRepository.Setup(repo => repo.DeleteOrderDetailAsync(orderDetailId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteOrderDetail(orderDetailId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteOrderDetail_ReturnsNotFound_WhenOrderDetailDoesNotExist()
        {
            // Arrange
            var orderDetailId = 99;

            _mockOrderDetailRepository.Setup(repo => repo.OrderDetailExistsAsync(orderDetailId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteOrderDetail(orderDetailId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        private List<OrderDetail> GetSampleOrderDetails()
        {
            return new List<OrderDetail>
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
                },
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
            };
        }

        private List<OrderDetailDto> GetSampleOrderDetailDtos()
        {
            return new List<OrderDetailDto>
            {
                new OrderDetailDto
                {
                    Id = 1,
                    OrderId = 1,
                    ProductName = "Product A",
                    Quantity = 2,
                    Price = 50.00m
                },
                new OrderDetailDto
                {
                    Id = 2,
                    OrderId = 1,
                    ProductName = "Product B",
                    Quantity = 1,
                    Price = 50.00m
                },
                new OrderDetailDto
                {
                    Id = 3,
                    OrderId = 2,
                    ProductName = "Product C",
                    Quantity = 4,
                    Price = 25.00m
                },
                new OrderDetailDto
                {
                    Id = 4,
                    OrderId = 2,
                    ProductName = "Product D",
                    Quantity = 2,
                    Price = 50.00m
                }
            };
        }
    }
}