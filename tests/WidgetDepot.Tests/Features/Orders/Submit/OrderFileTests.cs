using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.Submit;

namespace WidgetDepot.Tests.Features.Orders.Submit;

public class OrderFileTests
{
    private static Order BuildOrder(int orderId = 42)
    {
        var order = new Order
        {
            Id = orderId,
            CustomerId = 1,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            SubmittedAt = new DateTime(2026, 5, 10, 12, 0, 0, DateTimeKind.Utc),
            ShippingAddress = new Address
            {
                RecipientName = "Alice Smith",
                StreetLine1 = "123 Main St",
                StreetLine2 = "Apt 4",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701"
            },
            BillingAddress = new Address
            {
                RecipientName = "Alice Smith",
                StreetLine1 = "456 Oak Ave",
                City = "Shelbyville",
                State = "IL",
                ZipCode = "62565"
            }
        };

        var widget = new Widget { Id = 7, Sku = "W-001", Name = "Sprocket", Weight = 2.5m };
        order.Items.Add(new OrderItem { WidgetId = widget.Id, Widget = widget, Quantity = 3 });

        return order;
    }

    [Fact]
    public void FileName_IsZeroPaddedOrderId()
    {
        var order = BuildOrder(orderId: 42);

        var file = new OrderFile(order, "alice@example.com");

        file.FileName.ShouldBe("EXT-0000000042.TXT");
    }

    [Fact]
    public void Content_HeaderLine_ContainsZeroPaddedOrderId()
    {
        var order = BuildOrder(orderId: 42);

        var file = new OrderFile(order, "alice@example.com");

        var header = GetLine(file.Content, 0);
        header[..10].ShouldBe("0000000042");
    }

    [Fact]
    public void Content_HeaderLine_ContainsCustomerEmail()
    {
        var order = BuildOrder();

        var file = new OrderFile(order, "alice@example.com");

        var header = GetLine(file.Content, 0);
        header.Substring(10, 100).TrimEnd().ShouldBe("alice@example.com");
    }

    [Fact]
    public void Content_HeaderLine_ContainsSubmittedAtDate()
    {
        var order = BuildOrder();

        var file = new OrderFile(order, "alice@example.com");

        var header = GetLine(file.Content, 0);
        header.Substring(110, 10).TrimEnd().ShouldBe("2026/05/10");
    }

    [Fact]
    public void Content_HeaderLine_ContainsTotalWeight()
    {
        var order = BuildOrder();

        var file = new OrderFile(order, "alice@example.com");

        // 3 items × 2.5 weight = 7.5 total
        var header = GetLine(file.Content, 0);
        header.Substring(120, 10).ShouldBe("0000007.50");
    }

    [Fact]
    public void Content_ShippingAddressLine_HasAddressTypeS()
    {
        var order = BuildOrder();

        var file = new OrderFile(order, "alice@example.com");

        var line = GetLine(file.Content, 1);
        line.Substring(10, 10).TrimEnd().ShouldBe("S");
    }

    [Fact]
    public void Content_BillingAddressLine_HasAddressTypeB()
    {
        var order = BuildOrder();

        var file = new OrderFile(order, "alice@example.com");

        var line = GetLine(file.Content, 2);
        line.Substring(10, 10).TrimEnd().ShouldBe("B");
    }

    [Fact]
    public void Content_LineItem_ContainsZeroPaddedWidgetId()
    {
        var order = BuildOrder();

        var file = new OrderFile(order, "alice@example.com");

        var line = GetLine(file.Content, 3);
        line.Substring(10, 10).ShouldBe("0000000007");
    }

    [Fact]
    public void Content_LineItem_ContainsZeroPaddedQuantity()
    {
        var order = BuildOrder();

        var file = new OrderFile(order, "alice@example.com");

        var line = GetLine(file.Content, 3);
        line.Substring(20, 10).ShouldBe("0000000003");
    }

    [Fact]
    public void Content_HasOneLineItemPerOrderItem()
    {
        var order = BuildOrder();
        var widget2 = new Widget { Id = 8, Sku = "W-002", Name = "Cog", Weight = 1.0m };
        order.Items.Add(new OrderItem { WidgetId = widget2.Id, Widget = widget2, Quantity = 1 });

        var file = new OrderFile(order, "alice@example.com");

        var lines = file.Content.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        lines.Length.ShouldBe(5); // header + shipping + billing + 2 items
    }

    private static string GetLine(string content, int index) =>
        content.Split("\r\n")[index];
}