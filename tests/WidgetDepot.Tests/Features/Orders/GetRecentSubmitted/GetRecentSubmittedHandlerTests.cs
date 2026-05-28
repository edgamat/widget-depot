using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.GetRecentSubmitted;

namespace WidgetDepot.Tests.Features.Orders.GetRecentSubmitted;

public class GetRecentSubmittedHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Order> SeedOrderAsync(
        AppDbContext db,
        int customerId,
        OrderStatus status,
        DateTime? submittedAt = null,
        TransmissionStatus? transmissionStatus = null,
        DateTime? transmissionStatusChangedAt = null)
    {
        var order = new Order
        {
            CustomerId = customerId,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            SubmittedAt = submittedAt,
            TransmissionStatus = transmissionStatus,
            TransmissionStatusChangedAt = transmissionStatusChangedAt
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return order;
    }

    [Fact]
    public async Task HandleAsync_NoSubmittedOrders_ReturnsEmptyList()
    {
        using var db = CreateDb();
        var handler = new GetRecentSubmittedHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_CustomerHasSubmittedOrders_ReturnsOrders()
    {
        using var db = CreateDb();
        var submittedAt = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        var order = await SeedOrderAsync(db, customerId: 1, OrderStatus.Submitted, submittedAt);
        db.OrderItems.Add(new OrderItem { OrderId = order.Id, WidgetId = 10, Quantity = 4 });
        db.OrderItems.Add(new OrderItem { OrderId = order.Id, WidgetId = 11, Quantity = 1 });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new GetRecentSubmittedHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(order.Id);
        result[0].WidgetCount.ShouldBe(5);
        result[0].SubmittedAt.ShouldBe(submittedAt);
    }

    [Fact]
    public async Task HandleAsync_DraftOrdersExist_ExcludesDrafts()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, customerId: 1, OrderStatus.Draft);
        var submitted = await SeedOrderAsync(db, customerId: 1, OrderStatus.Submitted, DateTime.UtcNow);
        var handler = new GetRecentSubmittedHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(submitted.Id);
    }

    [Fact]
    public async Task HandleAsync_OrdersBelongToOtherCustomers_ReturnsOnlyOwnOrders()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, customerId: 2, OrderStatus.Submitted, DateTime.UtcNow);
        var ownOrder = await SeedOrderAsync(db, customerId: 1, OrderStatus.Submitted, DateTime.UtcNow);
        var handler = new GetRecentSubmittedHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(ownOrder.Id);
    }

    [Fact]
    public async Task HandleAsync_MoreThan10SubmittedOrders_ReturnsOnly10()
    {
        using var db = CreateDb();
        for (var i = 0; i < 12; i++)
        {
            await SeedOrderAsync(db, customerId: 1, OrderStatus.Submitted, DateTime.UtcNow.AddDays(-i));
        }
        var handler = new GetRecentSubmittedHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result.Count.ShouldBe(10);
    }

    [Fact]
    public async Task HandleAsync_MultipleSubmittedOrders_ReturnsSortedNewestFirst()
    {
        using var db = CreateDb();
        var older = await SeedOrderAsync(db, customerId: 1, OrderStatus.Submitted, new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));
        var newer = await SeedOrderAsync(db, customerId: 1, OrderStatus.Submitted, new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        var handler = new GetRecentSubmittedHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result[0].Id.ShouldBe(newer.Id);
        result[1].Id.ShouldBe(older.Id);
    }

    [Fact]
    public async Task HandleAsync_OrderWithShippingEstimate_ReturnsShippingEstimate()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, OrderStatus.Submitted, DateTime.UtcNow);
        order.ShippingEstimate = 19.99m;
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new GetRecentSubmittedHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result[0].ShippingEstimate.ShouldBe(19.99m);
    }

    [Fact]
    public async Task HandleAsync_PendingTransmission_ReturnsPendingStatus()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, customerId: 1, OrderStatus.Submitted, DateTime.UtcNow, TransmissionStatus.Pending);
        var handler = new GetRecentSubmittedHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result[0].TransmissionStatus.ShouldBe(TransmissionStatus.Pending);
        result[0].TransmissionStatusChangedAt.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_TransmittedOrder_ReturnsStatusAndTimestamp()
    {
        using var db = CreateDb();
        var changedAt = new DateTime(2026, 5, 15, 10, 0, 0, DateTimeKind.Utc);
        await SeedOrderAsync(db, customerId: 1, OrderStatus.Submitted, DateTime.UtcNow, TransmissionStatus.Transmitted, changedAt);
        var handler = new GetRecentSubmittedHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result[0].TransmissionStatus.ShouldBe(TransmissionStatus.Transmitted);
        result[0].TransmissionStatusChangedAt.ShouldBe(changedAt);
    }

    [Fact]
    public async Task HandleAsync_FailedTransmission_ReturnsStatusAndTimestamp()
    {
        using var db = CreateDb();
        var changedAt = new DateTime(2026, 5, 15, 10, 0, 0, DateTimeKind.Utc);
        await SeedOrderAsync(db, customerId: 1, OrderStatus.Submitted, DateTime.UtcNow, TransmissionStatus.Failed, changedAt);
        var handler = new GetRecentSubmittedHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result[0].TransmissionStatus.ShouldBe(TransmissionStatus.Failed);
        result[0].TransmissionStatusChangedAt.ShouldBe(changedAt);
    }

    [Fact]
    public async Task HandleAsync_MissingTransmission_ReturnsStatusAndTimestamp()
    {
        using var db = CreateDb();
        var changedAt = new DateTime(2026, 5, 15, 10, 0, 0, DateTimeKind.Utc);
        await SeedOrderAsync(db, customerId: 1, OrderStatus.Submitted, DateTime.UtcNow, TransmissionStatus.Missing, changedAt);
        var handler = new GetRecentSubmittedHandler(db);

        var result = await handler.HandleAsync(1, TestContext.Current.CancellationToken);

        result[0].TransmissionStatus.ShouldBe(TransmissionStatus.Missing);
        result[0].TransmissionStatusChangedAt.ShouldBe(changedAt);
    }
}