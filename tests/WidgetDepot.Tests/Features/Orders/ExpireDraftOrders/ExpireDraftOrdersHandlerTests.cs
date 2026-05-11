using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.ExpireDraftOrders;

namespace WidgetDepot.Tests.Features.Orders.ExpireDraftOrders;

public class ExpireDraftOrdersHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedOrderAsync(AppDbContext db, OrderStatus status, DateTime createdAt)
    {
        var order = new Order
        {
            CustomerId = 1,
            Status = status,
            CreatedAt = createdAt
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task HandleAsync_NoDraftOrders_ReturnsZero()
    {
        using var db = CreateDb();
        var handler = new ExpireDraftOrdersHandler(db);

        var result = await handler.HandleAsync(TestContext.Current.CancellationToken);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task HandleAsync_DraftOrderOlderThan30Days_ReturnsDeletedCount()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, OrderStatus.Draft, DateTime.UtcNow.AddDays(-31));
        var handler = new ExpireDraftOrdersHandler(db);

        var result = await handler.HandleAsync(TestContext.Current.CancellationToken);

        result.ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_DraftOrderOlderThan30Days_DeletesOrder()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, OrderStatus.Draft, DateTime.UtcNow.AddDays(-31));
        var handler = new ExpireDraftOrdersHandler(db);

        await handler.HandleAsync(TestContext.Current.CancellationToken);

        var remaining = await db.Orders.CountAsync(TestContext.Current.CancellationToken);
        remaining.ShouldBe(0);
    }

    [Fact]
    public async Task HandleAsync_DraftOrderExactly30DaysOld_DoesNotDelete()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, OrderStatus.Draft, DateTime.UtcNow.AddDays(-30).AddMinutes(1));
        var handler = new ExpireDraftOrdersHandler(db);

        var result = await handler.HandleAsync(TestContext.Current.CancellationToken);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task HandleAsync_SubmittedOrderOlderThan30Days_DoesNotDelete()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, OrderStatus.Submitted, DateTime.UtcNow.AddDays(-60));
        var handler = new ExpireDraftOrdersHandler(db);

        var result = await handler.HandleAsync(TestContext.Current.CancellationToken);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task HandleAsync_SubmittedOrderOlderThan30Days_LeavesOrderIntact()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, OrderStatus.Submitted, DateTime.UtcNow.AddDays(-60));
        var handler = new ExpireDraftOrdersHandler(db);

        await handler.HandleAsync(TestContext.Current.CancellationToken);

        var remaining = await db.Orders.CountAsync(TestContext.Current.CancellationToken);
        remaining.ShouldBe(1);
    }

    [Fact]
    public async Task HandleAsync_RecentDraftOrder_DoesNotDelete()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, OrderStatus.Draft, DateTime.UtcNow.AddDays(-5));
        var handler = new ExpireDraftOrdersHandler(db);

        var result = await handler.HandleAsync(TestContext.Current.CancellationToken);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task HandleAsync_MixedOrders_DeletesOnlyExpiredDrafts()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, OrderStatus.Draft, DateTime.UtcNow.AddDays(-31));   // expired draft
        await SeedOrderAsync(db, OrderStatus.Draft, DateTime.UtcNow.AddDays(-5));    // recent draft
        await SeedOrderAsync(db, OrderStatus.Submitted, DateTime.UtcNow.AddDays(-60)); // old submitted
        var handler = new ExpireDraftOrdersHandler(db);

        var result = await handler.HandleAsync(TestContext.Current.CancellationToken);

        result.ShouldBe(1);
        var remaining = await db.Orders.CountAsync(TestContext.Current.CancellationToken);
        remaining.ShouldBe(2);
    }

    [Fact]
    public async Task HandleAsync_CalledTwice_IsIdempotent()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, OrderStatus.Draft, DateTime.UtcNow.AddDays(-31));
        var handler = new ExpireDraftOrdersHandler(db);

        await handler.HandleAsync(TestContext.Current.CancellationToken);
        var secondResult = await handler.HandleAsync(TestContext.Current.CancellationToken);

        secondResult.ShouldBe(0);
    }
}