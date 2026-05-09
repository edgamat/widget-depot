using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.DeleteDraft;

namespace WidgetDepot.Tests.Features.Orders.DeleteDraft;

public class DeleteDraftHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Order> SeedOrderAsync(AppDbContext db, int customerId, OrderStatus status = OrderStatus.Draft)
    {
        var order = new Order
        {
            CustomerId = customerId,
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return order;
    }

    [Fact]
    public async Task HandleAsync_OrderDoesNotExist_ReturnsOrderNotFoundError()
    {
        using var db = CreateDb();
        var handler = new DeleteDraftHandler(db);

        var result = await handler.HandleAsync(999, 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<DeleteDraftError.OrderNotFound>();
    }

    [Fact]
    public async Task HandleAsync_OrderBelongsToOtherCustomer_ReturnsForbiddenError()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 2);
        var handler = new DeleteDraftHandler(db);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<DeleteDraftError.Forbidden>();
    }

    [Fact]
    public async Task HandleAsync_ValidOrder_ReturnsNull()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1);
        var handler = new DeleteDraftHandler(db);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_ValidOrder_DeletesOrder()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1);
        var handler = new DeleteDraftHandler(db);

        await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        var deleted = await db.Orders.FindAsync([order.Id], TestContext.Current.CancellationToken);
        deleted.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_OrderBelongsToOtherCustomer_DoesNotDeleteOrder()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 2);
        var handler = new DeleteDraftHandler(db);

        await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        var existing = await db.Orders.FindAsync([order.Id], TestContext.Current.CancellationToken);
        existing.ShouldNotBeNull();
    }
}