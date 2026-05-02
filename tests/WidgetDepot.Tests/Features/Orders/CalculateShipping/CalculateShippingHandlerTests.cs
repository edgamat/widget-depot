using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using Moq;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.CalculateShipping;

namespace WidgetDepot.Tests.Features.Orders.CalculateShipping;

public class CalculateShippingHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static IConfiguration EmptyConfiguration() =>
        new ConfigurationBuilder().Build();

    private static CalculateShippingHandler CreateHandler(
        AppDbContext db,
        IShippingApiClient shippingApiClient,
        IConfiguration? configuration = null) =>
        new(db, shippingApiClient, configuration ?? EmptyConfiguration());

    private static async Task<Order> SeedOrderWithShippingAddressAsync(AppDbContext db, int customerId = 1)
    {
        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            ShippingAddress = new Address
            {
                RecipientName = "Alice Smith",
                StreetLine1 = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701"
            }
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return order;
    }

    private static async Task<(Order order, Widget widget)> SeedOrderWithItemsAsync(
        AppDbContext db,
        int customerId = 1,
        decimal widgetWeight = 2.5m,
        int quantity = 3)
    {
        var widget = new Widget
        {
            Id = 1,
            Sku = "WGT-001",
            Name = "Test Widget",
            Weight = widgetWeight
        };
        db.Widgets.Add(widget);

        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            ShippingAddress = new Address
            {
                RecipientName = "Alice Smith",
                StreetLine1 = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701"
            }
        };
        order.Items.Add(new OrderItem { WidgetId = widget.Id, Quantity = quantity });
        db.Orders.Add(order);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (order, widget);
    }

    [Fact]
    public async Task HandleAsync_SuccessfulApiResponse_ReturnsEstimate()
    {
        using var db = CreateDb();
        var (order, _) = await SeedOrderWithItemsAsync(db, widgetWeight: 2.0m, quantity: 3);

        var mockClient = new Mock<IShippingApiClient>();
        mockClient.Setup(c => c.GetEstimateAsync(It.IsAny<ShippingEstimateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingEstimateResult.Success(19.99m, "USD"));

        var handler = CreateHandler(db, mockClient.Object);
        var result = await handler.HandleAsync(order.Id, 1, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<CalculateShippingResponse>();
        response.EstimatedCost.ShouldBe(19.99m);
        response.Currency.ShouldBe("USD");
    }

    [Fact]
    public async Task HandleAsync_SuccessfulApiResponse_StoresEstimateOnOrder()
    {
        using var db = CreateDb();
        var (order, _) = await SeedOrderWithItemsAsync(db);

        var mockClient = new Mock<IShippingApiClient>();
        mockClient.Setup(c => c.GetEstimateAsync(It.IsAny<ShippingEstimateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingEstimateResult.Success(14.50m, "USD"));

        var handler = CreateHandler(db, mockClient.Object);
        await handler.HandleAsync(order.Id, 1, TestContext.Current.CancellationToken);

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.ShippingEstimate.ShouldBe(14.50m);
    }

    [Fact]
    public async Task HandleAsync_MultipleItems_SendsCorrectTotalWeight()
    {
        using var db = CreateDb();
        var widget1 = new Widget { Id = 1, Sku = "W-001", Name = "Widget 1", Weight = 2.0m };
        var widget2 = new Widget { Id = 2, Sku = "W-002", Name = "Widget 2", Weight = 3.0m };
        db.Widgets.AddRange(widget1, widget2);

        var order = new Order
        {
            CustomerId = 1,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            ShippingAddress = new Address
            {
                RecipientName = "Bob Jones",
                StreetLine1 = "456 Oak Ave",
                City = "Shelbyville",
                State = "IL",
                ZipCode = "62565"
            }
        };
        order.Items.Add(new OrderItem { WidgetId = 1, Quantity = 2 });
        order.Items.Add(new OrderItem { WidgetId = 2, Quantity = 4 });
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        ShippingEstimateRequest? capturedRequest = null;
        var mockClient = new Mock<IShippingApiClient>();
        mockClient.Setup(c => c.GetEstimateAsync(It.IsAny<ShippingEstimateRequest>(), It.IsAny<CancellationToken>()))
            .Callback<ShippingEstimateRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new ShippingEstimateResult.Success(25.00m, "USD"));

        var handler = CreateHandler(db, mockClient.Object);
        await handler.HandleAsync(order.Id, 1, TestContext.Current.CancellationToken);

        // widget1: 2 × 2.0 = 4.0, widget2: 4 × 3.0 = 12.0 → total = 16.0
        capturedRequest.ShouldNotBeNull();
        capturedRequest.WeightLbs.ShouldBe(16.0m);
    }

    [Fact]
    public async Task HandleAsync_DestinationAddressSentToApi()
    {
        using var db = CreateDb();
        var (order, _) = await SeedOrderWithItemsAsync(db);

        ShippingEstimateRequest? capturedRequest = null;
        var mockClient = new Mock<IShippingApiClient>();
        mockClient.Setup(c => c.GetEstimateAsync(It.IsAny<ShippingEstimateRequest>(), It.IsAny<CancellationToken>()))
            .Callback<ShippingEstimateRequest, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new ShippingEstimateResult.Success(10.00m, "USD"));

        var handler = CreateHandler(db, mockClient.Object);
        await handler.HandleAsync(order.Id, 1, TestContext.Current.CancellationToken);

        capturedRequest.ShouldNotBeNull();
        capturedRequest.DestinationPostalCode.ShouldBe("62701");
        capturedRequest.DestinationCountry.ShouldBe("US");
    }

    [Fact]
    public async Task HandleAsync_OrderNotFound_ReturnsOrderNotFound()
    {
        using var db = CreateDb();
        var mockClient = new Mock<IShippingApiClient>();
        var handler = CreateHandler(db, mockClient.Object);

        var result = await handler.HandleAsync(999, 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<CalculateShippingError.OrderNotFound>();
    }

    [Fact]
    public async Task HandleAsync_OrderBelongsToDifferentCustomer_ReturnsForbidden()
    {
        using var db = CreateDb();
        var (order, _) = await SeedOrderWithItemsAsync(db, customerId: 1);

        var mockClient = new Mock<IShippingApiClient>();
        var handler = CreateHandler(db, mockClient.Object);

        var result = await handler.HandleAsync(order.Id, 2, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<CalculateShippingError.Forbidden>();
    }

    [Fact]
    public async Task HandleAsync_NoShippingAddress_ReturnsNoShippingAddress()
    {
        using var db = CreateDb();
        var order = new Order
        {
            CustomerId = 1,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var mockClient = new Mock<IShippingApiClient>();
        var handler = CreateHandler(db, mockClient.Object);

        var result = await handler.HandleAsync(order.Id, 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<CalculateShippingError.NoShippingAddress>();
    }

    [Fact]
    public async Task HandleAsync_ShippingApiFailure_ReturnsShippingApiFailure()
    {
        using var db = CreateDb();
        var (order, _) = await SeedOrderWithItemsAsync(db);

        var mockClient = new Mock<IShippingApiClient>();
        mockClient.Setup(c => c.GetEstimateAsync(It.IsAny<ShippingEstimateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingEstimateResult.Failure("Service unavailable"));

        var handler = CreateHandler(db, mockClient.Object);
        var result = await handler.HandleAsync(order.Id, 1, TestContext.Current.CancellationToken);

        var error = result.ShouldBeOfType<CalculateShippingError.ShippingApiFailure>();
        error.Reason.ShouldBe("Service unavailable");
    }

    [Fact]
    public async Task HandleAsync_ShippingApiFailure_DoesNotStoreEstimate()
    {
        using var db = CreateDb();
        var (order, _) = await SeedOrderWithItemsAsync(db);

        var mockClient = new Mock<IShippingApiClient>();
        mockClient.Setup(c => c.GetEstimateAsync(It.IsAny<ShippingEstimateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingEstimateResult.Failure("Timeout"));

        var handler = CreateHandler(db, mockClient.Object);
        await handler.HandleAsync(order.Id, 1, TestContext.Current.CancellationToken);

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.ShippingEstimate.ShouldBeNull();
    }
}