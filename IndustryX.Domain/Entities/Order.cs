using IndustryX.Domain.ValueObjects;

namespace IndustryX.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Address info
        public OrderAddress BillingAddress { get; set; }
        public OrderAddress ShippingAddress { get; set; }

        // Payment info
        public string? PaymentProvider { get; set; }
        public string? PaymentTransactionId { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public DateTime? PaymentDate { get; set; }
        public bool IsPaid { get; set; } = false;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }
}
