using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace OrderManagementAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string CustomerName { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }

    public enum OrderStatus
    {
        Pending = 0,
        Completed = 1,
        Canceled = 2
    }
}