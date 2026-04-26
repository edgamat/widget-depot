namespace WidgetDepot.ApiService.Data;

public enum OrderStatus
{
    Draft = 0,
    Submitted = 1,
    Cancelled = 2
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
}