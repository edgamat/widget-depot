using System.Text;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Orders.Submit;

public class OrderFile
{
    private readonly Order _order;
    private readonly string _customerEmail;

    public string FileName { get; }

    public OrderFile(Order order, string customerEmail)
    {
        FileName = $"EXT-{order.Id.ToString().PadLeft(10, '0')}.TXT";
        _order = order;
        _customerEmail = customerEmail;
    }

    public string GetContent() => BuildContent(_order, _customerEmail);

    private static string BuildContent(Order order, string customerEmail)
    {
        var sb = new StringBuilder();

        var totalWeight = order.Items.Sum(i => i.Quantity * i.Widget.Weight);
        var dateSubmitted = (order.SubmittedAt ?? DateTime.UtcNow).ToString("yyyy/MM/dd");

        sb.Append(ZeroPad(order.Id.ToString(), 10));
        sb.Append(PadRight(customerEmail, 100));
        sb.Append(PadRight(dateSubmitted, 10));
        sb.Append(totalWeight.ToString("0000000.00"));
        sb.Append("\r\n");

        AppendAddress(sb, order.Id, "S", order.ShippingAddress!);
        AppendAddress(sb, order.Id, "B", order.BillingAddress!);

        foreach (var item in order.Items)
        {
            sb.Append(ZeroPad(order.Id.ToString(), 10));
            sb.Append(ZeroPad(item.WidgetId.ToString(), 10));
            sb.Append(ZeroPad(item.Quantity.ToString(), 10));
            sb.Append("\r\n");
        }

        return sb.ToString();
    }

    private static void AppendAddress(StringBuilder sb, int orderId, string addressType, Address address)
    {
        sb.Append(ZeroPad(orderId.ToString(), 10));
        sb.Append(PadRight(addressType, 10));
        sb.Append(PadRight(address.StreetLine1, 200));
        sb.Append(PadRight(address.StreetLine2 ?? "", 200));
        sb.Append(PadRight(address.City, 100));
        sb.Append(PadRight(address.State, 2));
        sb.Append(PadRight(address.ZipCode, 10));
        sb.Append("\r\n");
    }

    private static string PadRight(string value, int width) =>
        value.PadRight(width)[..width];

    private static string ZeroPad(string value, int width) =>
        value.PadLeft(width, '0')[^width..];
}