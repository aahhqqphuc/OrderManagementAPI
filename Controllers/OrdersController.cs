using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OrderManagementAPI.DTOs;
using OrderManagementAPI.Models;
using OrderManagementAPI.Repositories;

namespace OrderManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderRepository orderRepository,
            IMapper mapper,
            ILogger<OrdersController> logger)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Getting all orders for page {Page} with page size {PageSize}", page, pageSize);

            var orders = await _orderRepository.GetAllOrdersAsync(page, pageSize);
            var totalCount = await _orderRepository.GetTotalOrderCountAsync();

            var orderDtos = _mapper.Map<IEnumerable<OrderDto>>(orders);

            Response.Headers.Add("X-Total-Count", totalCount.ToString());
            Response.Headers.Add("X-Page", page.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(orderDtos);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            _logger.LogInformation("Getting order with ID {OrderId}", id);

            var order = await _orderRepository.GetOrderByIdAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found", id);
                return NotFound();
            }

            return Ok(_mapper.Map<OrderDto>(order));
        }

        // POST: api/orders
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
        {
            _logger.LogInformation("Creating a new order for customer {CustomerName}", createOrderDto.CustomerName);

            var order = _mapper.Map<Order>(createOrderDto);

            if (createOrderDto.OrderDetails != null && createOrderDto.OrderDetails.Any())
            {
                order.OrderDetails = _mapper.Map<List<OrderDetail>>(createOrderDto.OrderDetails);
            }

            var createdOrder = await _orderRepository.CreateOrderAsync(order);

            var orderDto = _mapper.Map<OrderDto>(createdOrder);

            return CreatedAtAction(nameof(GetOrder), new { id = orderDto.Id }, orderDto);
        }

        // PUT: api/orders/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, UpdateOrderDto updateOrderDto)
        {
            _logger.LogInformation("Updating order with ID {OrderId}", id);

            var existingOrder = await _orderRepository.GetOrderByIdAsync(id);

            if (existingOrder == null)
            {
                _logger.LogWarning("Order with ID {OrderId} not found", id);
                return NotFound();
            }

            var order = _mapper.Map(updateOrderDto, existingOrder);

            var updatedOrder = await _orderRepository.UpdateOrderAsync(order);

            return NoContent();
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            _logger.LogInformation("Deleting order with ID {OrderId}", id);

            var result = await _orderRepository.DeleteOrderAsync(id);

            if (!result)
            {
                _logger.LogWarning("Order with ID {OrderId} not found", id);
                return NotFound();
            }

            return NoContent();
        }
    }
}