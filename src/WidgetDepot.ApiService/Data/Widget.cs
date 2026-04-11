namespace WidgetDepot.ApiService.Data;

public class Widget
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Price { get; set; }
    public DateOnly DateAvailable { get; set; }
}