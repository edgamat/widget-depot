namespace WidgetDepot.ApiService.Data;

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int WidgetId { get; set; }
    public int Quantity { get; set; }
    public decimal WidgetWeight { get; set; }
}