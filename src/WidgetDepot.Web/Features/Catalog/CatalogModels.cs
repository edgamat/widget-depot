namespace WidgetDepot.Web.Features.Catalog;

public record WidgetResult(
    int Id,
    string Sku,
    string Name,
    string Description,
    string Manufacturer,
    decimal Weight,
    decimal Price,
    DateOnly DateAvailable);