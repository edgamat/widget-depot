using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.SaveAddresses;

namespace WidgetDepot.Tests.Features.Orders.SaveAddresses;

public class SaveAddressesHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static SaveAddressesRequest ValidRequest() => new(
        ShippingAddress: new AddressRequest("Alice Smith", "123 Main St", null, "Springfield", "IL", "62701"),
        BillingAddress: new AddressRequest("Bob Jones", "456 Oak Ave", "Apt 2", "Shelbyville", "IL", "62565-1234"));

    private static async Task<Order> SeedOrderAsync(AppDbContext db, int customerId = 1)
    {
        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return order;
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_SavesAddressesToOrder()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1);

        var handler = new SaveAddressesHandler(db);
        var result = await handler.HandleAsync(order.Id, 1, ValidRequest(), TestContext.Current.CancellationToken);

        result.ShouldBeNull();
        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.ShippingAddress.ShouldNotBeNull();
        saved.ShippingAddress.RecipientName.ShouldBe("Alice Smith");
        saved.BillingAddress.ShouldNotBeNull();
        saved.BillingAddress.RecipientName.ShouldBe("Bob Jones");
    }

    [Fact]
    public async Task HandleAsync_OrderNotFound_ReturnsOrderNotFound()
    {
        using var db = CreateDb();
        var handler = new SaveAddressesHandler(db);

        var result = await handler.HandleAsync(999, 1, ValidRequest(), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SaveAddressesError.OrderNotFound>();
    }

    [Fact]
    public async Task HandleAsync_OrderBelongsToDifferentCustomer_ReturnsForbidden()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1);

        var handler = new SaveAddressesHandler(db);
        var result = await handler.HandleAsync(order.Id, 2, ValidRequest(), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SaveAddressesError.Forbidden>();
    }

    [Theory]
    [InlineData("shippingAddress.recipientName")]
    [InlineData("shippingAddress.streetLine1")]
    [InlineData("shippingAddress.city")]
    public async Task HandleAsync_ShippingFieldExceeds100Chars_ReturnsInvalidRequest(string expectedErrorKey)
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db);
        var longValue = new string('x', 101);

        var request = expectedErrorKey switch
        {
            "shippingAddress.recipientName" => ValidRequest() with { ShippingAddress = ValidRequest().ShippingAddress with { RecipientName = longValue } },
            "shippingAddress.streetLine1" => ValidRequest() with { ShippingAddress = ValidRequest().ShippingAddress with { StreetLine1 = longValue } },
            "shippingAddress.city" => ValidRequest() with { ShippingAddress = ValidRequest().ShippingAddress with { City = longValue } },
            _ => throw new InvalidOperationException()
        };

        var handler = new SaveAddressesHandler(db);
        var result = await handler.HandleAsync(order.Id, 1, request, TestContext.Current.CancellationToken);

        var error = result.ShouldBeOfType<SaveAddressesError.InvalidRequest>();
        error.Errors.ShouldContainKey(expectedErrorKey);
    }

    [Fact]
    public async Task HandleAsync_ShippingStreetLine2Exceeds100Chars_ReturnsInvalidRequest()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db);
        var request = ValidRequest() with { ShippingAddress = ValidRequest().ShippingAddress with { StreetLine2 = new string('x', 101) } };

        var handler = new SaveAddressesHandler(db);
        var result = await handler.HandleAsync(order.Id, 1, request, TestContext.Current.CancellationToken);

        var error = result.ShouldBeOfType<SaveAddressesError.InvalidRequest>();
        error.Errors.ShouldContainKey("shippingAddress.streetLine2");
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("12345-6789")]
    public async Task HandleAsync_ValidZipCode_Succeeds(string zipCode)
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db);
        var request = ValidRequest() with { ShippingAddress = ValidRequest().ShippingAddress with { ZipCode = zipCode } };

        var handler = new SaveAddressesHandler(db);
        var result = await handler.HandleAsync(order.Id, 1, request, TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("123456")]
    [InlineData("12345-678")]
    [InlineData("ABCDE")]
    [InlineData("")]
    public async Task HandleAsync_InvalidZipCode_ReturnsInvalidRequest(string zipCode)
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db);
        var request = ValidRequest() with { ShippingAddress = ValidRequest().ShippingAddress with { ZipCode = zipCode } };

        var handler = new SaveAddressesHandler(db);
        var result = await handler.HandleAsync(order.Id, 1, request, TestContext.Current.CancellationToken);

        var error = result.ShouldBeOfType<SaveAddressesError.InvalidRequest>();
        error.Errors.ShouldContainKey("shippingAddress.zipCode");
    }
}