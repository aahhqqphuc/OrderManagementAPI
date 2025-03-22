using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OrderManagementAPI.DTOs;
using OrderManagementAPI.Models;
using OrderManagementAPI.Repositories;

namespace OrderManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderDetailsController : ControllerBase
    {
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderDetailsController> _logger;

        public OrderDetailsController(
            IOrderDetailRepository orderDetailRepository,
            IOrderRepository orderRepository,
            IMapper mapper,
            ILogger<OrderDetailsController> logger)
        {
            _orderDetailRepository = orderDetailRepository;
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/orders/{id}/order-details
        [HttpGet("orders/{orderId}/order-details")]
        public async Task<ActionResult<IEnumerable<OrderDetailDto>>> GetOrderDetails(int orderId)
        {
            _logger.LogInformation("Getting order details for order ID {OrderId}", orderId);

            // Check if order exists
            if (!await _orderRepository.OrderExistsAsync(orderId))
            {
                _logger.LogWarning("Order with ID {OrderId} not found", orderId);
                return NotFound();
            }

            var orderDetails = await _orderDetailRepository.GetOrderDetailsByOrderIdAsync(orderId);
            var orderDetailDtos = _mapper.Map<IEnumerable<OrderDetailDto>>(orderDetails);

            return Ok(orderDetailDtos);
        }

        // POST: api/orders/{id}/order-details
        [HttpPost("orders/{orderId}/order-details")]
        public async Task<ActionResult<OrderDetailDto>> CreateOrderDetail(int orderId, CreateOrderDetailDto createOrderDetailDto)
        {
            _logger.LogInformation("Adding product {ProductName} to order ID {OrderId}", createOrderDetailDto.ProductName, orderId);

            // Check if order exists
            if (!await _orderRepository.OrderExistsAsync(orderId))
            {
                _logger.LogWarning("Order with ID {OrderId} not found", orderId);
                return NotFound();
            }

            var orderDetail = _mapper.Map<OrderDetail>(createOrderDetailDto);
            orderDetail.OrderId = orderId;

            var createdOrderDetail = await _orderDetailRepository.CreateOrderDetailAsync(orderDetail);

            var orderDetailDto = _mapper.Map<OrderDetailDto>(createdOrderDetail);

            return CreatedAtAction(nameof(GetOrderDetails), new { orderId }, orderDetailDto);
        }

        // DELETE: api/order-details/{id}
        [HttpDelete("order-details/{id}")]
        public async Task<IActionResult> DeleteOrderDetail(int id)
        {
            _logger.LogInformation("Deleting order detail with ID {OrderDetailId}", id);

            var result = await _orderDetailRepository.DeleteOrderDetailAsync(id);

            if (!result)
            {
                _logger.LogWarning("Order detail with ID {OrderDetailId} not found", id);
                return NotFound();
            }

            return NoContent();
        }
    }
}