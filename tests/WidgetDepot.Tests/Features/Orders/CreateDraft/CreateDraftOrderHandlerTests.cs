using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.CreateDraft;

namespace WidgetDepot.Tests.Features.Orders.CreateDraft;

public class CreateDraftOrderHandlerTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_CreatesOrderWithDraftStatus()
    {
        using var db = CreateDb();
        var widget = new Widget { Id = 1, Name = "Sprocket", Sku = "SPR-001", Weight = 1.5m };
        db.Widgets.Add(widget);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateDraftOrderHandler(db);
        var request = new CreateDraftOrderRequest([new CreateDraftOrderItemRequest(1, 2)]);

        var result = await handler.HandleAsync(42, request, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<CreateDraftOrderResponse>();
        var order = await db.Orders.Include(o => o.Items).FirstAsync(TestContext.Current.CancellationToken);
        order.CustomerId.ShouldBe(42);
        order.Status.ShouldBe(OrderStatus.Draft);
        response.OrderId.ShouldBe(order.Id);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_CreatesOrderItemsWithCorrectQuantity()
    {
        using var db = CreateDb();
        db.Widgets.Add(new Widget { Id = 1, Name = "Sprocket", Sku = "SPR-001", Weight = 1.5m });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateDraftOrderHandler(db);
        var request = new CreateDraftOrderRequest([new CreateDraftOrderItemRequest(1, 3)]);

        await handler.HandleAsync(1, request, TestContext.Current.CancellationToken);

        var order = await db.Orders.Include(o => o.Items).FirstAsync(TestContext.Current.CancellationToken);
        order.Items.Count.ShouldBe(1);
        order.Items.First().Quantity.ShouldBe(3);
    }

    [Fact]
    public async Task HandleAsync_UnknownWidgetId_ReturnsWidgetNotFound()
    {
        using var db = CreateDb();
        var handler = new CreateDraftOrderHandler(db);
        var request = new CreateDraftOrderRequest([new CreateDraftOrderItemRequest(999, 1)]);

        var result = await handler.HandleAsync(1, request, TestContext.Current.CancellationToken);

        var error = result.ShouldBeOfType<CreateDraftOrderError.WidgetNotFound>();
        error.WidgetId.ShouldBe(999);
    }

    [Fact]
    public async Task HandleAsync_MultipleItems_CreatesAllOrderItems()
    {
        using var db = CreateDb();
        db.Widgets.AddRange(
            new Widget { Id = 1, Name = "Widget A", Sku = "A-001", Weight = 1.0m },
            new Widget { Id = 2, Name = "Widget B", Sku = "B-001", Weight = 2.0m }
        );
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new CreateDraftOrderHandler(db);
        var request = new CreateDraftOrderRequest([
            new CreateDraftOrderItemRequest(1, 2),
            new CreateDraftOrderItemRequest(2, 5)
        ]);

        await handler.HandleAsync(1, request, TestContext.Current.CancellationToken);

        var order = await db.Orders.Include(o => o.Items).FirstAsync(TestContext.Current.CancellationToken);
        order.Items.Count.ShouldBe(2);
    }
}