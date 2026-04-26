namespace WidgetDepot.ApiService.Data;

public class Address
{
    public string RecipientName { get; set; } = "";
    public string StreetLine1 { get; set; } = "";
    public string? StreetLine2 { get; set; }
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
}