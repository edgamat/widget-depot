using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.UpdateDraftItems;

namespace WidgetDepot.Tests.Features.Orders.UpdateDraftItems;

public class UpdateDraftItemsHandlerTests
{
    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_ReplacesExistingItems()
    {
        using var db = CreateDb();
        var widget1 = new Widget { Id = 1, Name = "Sprocket", Sku = "SPR-001", Weight = 1.0m };
        var widget2 = new Widget { Id = 2, Name = "Cog", Sku = "COG-001", Weight = 2.0m };
        db.Widgets.AddRange(widget1, widget2);
        var order = new Order
        {
            Id = 1,
            CustomerId = 42,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            Items = [new OrderItem { WidgetId = 1, Quantity = 3 }]
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateDraftItemsHandler(db);
        var request = new UpdateDraftItemsRequest([new UpdateDraftItemRequest(2, 5)]);

        var result = await handler.HandleAsync(1, 42, request, TestContext.Current.CancellationToken);

        result.ShouldBeNull();
        var updated = await db.Orders.Include(o => o.Items).FirstAsync(TestContext.Current.CancellationToken);
        updated.Items.Count.ShouldBe(1);
        updated.Items.First().WidgetId.ShouldBe(2);
        updated.Items.First().Quantity.ShouldBe(5);
    }

    [Fact]
    public async Task HandleAsync_OrderNotFound_ReturnsOrderNotFound()
    {
        using var db = CreateDb();
        var handler = new UpdateDraftItemsHandler(db);
        var request = new UpdateDraftItemsRequest([new UpdateDraftItemRequest(1, 1)]);

        var result = await handler.HandleAsync(999, 42, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateDraftItemsError.OrderNotFound>();
    }

    [Fact]
    public async Task HandleAsync_WrongCustomer_ReturnsForbidden()
    {
        using var db = CreateDb();
        db.Orders.Add(new Order { Id = 1, CustomerId = 42, Status = OrderStatus.Draft, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateDraftItemsHandler(db);
        var request = new UpdateDraftItemsRequest([new UpdateDraftItemRequest(1, 1)]);

        var result = await handler.HandleAsync(1, 99, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateDraftItemsError.Forbidden>();
    }

    [Fact]
    public async Task HandleAsync_NonDraftOrder_ReturnsNotDraft()
    {
        using var db = CreateDb();
        db.Orders.Add(new Order { Id = 1, CustomerId = 42, Status = OrderStatus.Submitted, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateDraftItemsHandler(db);
        var request = new UpdateDraftItemsRequest([new UpdateDraftItemRequest(1, 1)]);

        var result = await handler.HandleAsync(1, 42, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateDraftItemsError.NotDraft>();
    }

    [Fact]
    public async Task HandleAsync_UnknownWidgetId_ReturnsWidgetNotFound()
    {
        using var db = CreateDb();
        db.Orders.Add(new Order { Id = 1, CustomerId = 42, Status = OrderStatus.Draft, CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new UpdateDraftItemsHandler(db);
        var request = new UpdateDraftItemsRequest([new UpdateDraftItemRequest(999, 1)]);

        var result = await handler.HandleAsync(1, 42, request, TestContext.Current.CancellationToken);

        var error = result.ShouldBeOfType<UpdateDraftItemsError.WidgetNotFound>();
        error.WidgetId.ShouldBe(999);
    }
}