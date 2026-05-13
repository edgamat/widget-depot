using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.GetByOrderNumber;

namespace WidgetDepot.Tests.Features.Orders.GetByOrderNumber;

public class GetByOrderNumberHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Order> SeedOrderAsync(AppDbContext db, int customerId = 1, OrderStatus status = OrderStatus.Draft)
    {
        var order = new Order
        {
            CustomerId = customerId,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            SubmittedAt = status == OrderStatus.Submitted ? DateTime.UtcNow : null
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return order;
    }

    private static async Task<Widget> SeedWidgetAsync(AppDbContext db)
    {
        var widget = new Widget
        {
            Sku = "SPR-001",
            Name = "Sprocket",
            Description = "A sprocket",
            Manufacturer = "Acme",
            Weight = 1.5m,
            Price = 9.99m,
            DateAvailable = new DateOnly(2026, 1, 1)
        };
        db.Widgets.Add(widget);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return widget;
    }

    [Fact]
    public async Task HandleAsync_OrderNotFound_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = new GetByOrderNumberHandler(db);

        var result = await handler.HandleAsync(999, 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetByOrderNumberNotFound>();
    }

    [Fact]
    public async Task HandleAsync_OrderBelongsToDifferentCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1);
        var handler = new GetByOrderNumberHandler(db);

        var result = await handler.HandleAsync(order.Id, 2, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetByOrderNumberNotFound>();
    }

    [Fact]
    public async Task HandleAsync_DraftOrderBelongsToCustomer_ReturnsResponse()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, status: OrderStatus.Draft);
        var handler = new GetByOrderNumberHandler(db);

        var result = await handler.HandleAsync(order.Id, 1, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<GetByOrderNumberResponse>();
        response.Id.ShouldBe(order.Id);
        response.Status.ShouldBe("Draft");
        response.SubmittedAt.ShouldBeNull();
        response.Items.ShouldBeEmpty();
        response.ShippingAddress.ShouldBeNull();
        response.BillingAddress.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_SubmittedOrderBelongsToCustomer_ReturnsResponse()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, status: OrderStatus.Submitted);
        var handler = new GetByOrderNumberHandler(db);

        var result = await handler.HandleAsync(order.Id, 1, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<GetByOrderNumberResponse>();
        response.Status.ShouldBe("Submitted");
        response.SubmittedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task HandleAsync_OrderWithItems_ReturnsItemsWithWidgetDetails()
    {
        using var db = CreateDb();
        var widget = await SeedWidgetAsync(db);
        var order = await SeedOrderAsync(db, customerId: 1);
        db.OrderItems.Add(new OrderItem { OrderId = order.Id, WidgetId = widget.Id, Quantity = 3 });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new GetByOrderNumberHandler(db);

        var result = await handler.HandleAsync(order.Id, 1, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<GetByOrderNumberResponse>();
        response.Items.Count.ShouldBe(1);
        response.Items[0].Sku.ShouldBe("SPR-001");
        response.Items[0].Name.ShouldBe("Sprocket");
        response.Items[0].Weight.ShouldBe(1.5m);
        response.Items[0].UnitCost.ShouldBe(9.99m);
        response.Items[0].Quantity.ShouldBe(3);
    }
}