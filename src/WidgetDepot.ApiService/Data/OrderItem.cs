namespace WidgetDepot.ApiService.Data;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int WidgetId { get; set; }
    public Widget Widget { get; set; } = null!;
    public int Quantity { get; set; }
}