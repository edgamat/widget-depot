namespace WidgetDepot.ApiService.Data;

public enum OrderStatus
{
    Draft = 0,
    Submitted = 1,
    Cancelled = 2
}

public enum TransmissionStatus
{
    Pending = 0,
    Transmitted = 1,
    Failed = 2,
    Missing = 3
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
    public Address? ShippingAddress { get; set; }
    public Address? BillingAddress { get; set; }
    public decimal? ShippingEstimate { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public TransmissionStatus? TransmissionStatus { get; set; }
    public DateTime? TransmissionStatusChangedAt { get; set; }
}