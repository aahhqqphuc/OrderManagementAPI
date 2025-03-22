using OrderManagementAPI.Models;
using System.Text.Json.Serialization;

namespace OrderManagementAPI.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalAmount { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OrderStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; }
    }

    public class CreateOrderDto
    {
        public string CustomerName { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public List<CreateOrderDetailDto> OrderDetails { get; set; }
    }

    public class UpdateOrderDto
    {
        public string CustomerName { get; set; }
        public OrderStatus Status { get; set; }
    }
}