using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.GetDrafts;

namespace WidgetDepot.Tests.Features.Orders.GetDrafts;

public class GetDraftsHandlerTests
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
    public async Task HandleAsync_NoDraftOrders_ReturnsEmptyList()
    {
        using var db = CreateDb();
        var handler = new GetDraftsHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_CustomerHasDraftOrders_ReturnsDraftOrders()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1);
        db.OrderItems.Add(new OrderItem { OrderId = order.Id, WidgetId = 10, Quantity = 3 });
        db.OrderItems.Add(new OrderItem { OrderId = order.Id, WidgetId = 11, Quantity = 2 });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new GetDraftsHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(order.Id);
        result[0].WidgetCount.ShouldBe(5);
        result[0].CreatedAt.ShouldBe(order.CreatedAt);
    }

    [Fact]
    public async Task HandleAsync_OrdersBelongToOtherCustomers_ReturnsOnlyOwnDrafts()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, customerId: 2);
        var ownOrder = await SeedOrderAsync(db, customerId: 1);
        var handler = new GetDraftsHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(ownOrder.Id);
    }

    [Fact]
    public async Task HandleAsync_CustomerHasNonDraftOrders_ReturnsOnlyDrafts()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, customerId: 1, status: OrderStatus.Submitted);
        var draftOrder = await SeedOrderAsync(db, customerId: 1, status: OrderStatus.Draft);
        var handler = new GetDraftsHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(draftOrder.Id);
    }
}