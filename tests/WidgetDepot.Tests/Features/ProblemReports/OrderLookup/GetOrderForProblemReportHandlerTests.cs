using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.ProblemReports.GetOrderForProblemReport;

namespace WidgetDepot.Tests.Features.ProblemReports.OrderLookup;

public class GetOrderForProblemReportHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Order> SeedOrderAsync(AppDbContext db, int customerId = 1, OrderStatus status = OrderStatus.Submitted)
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

    private static async Task<Widget> SeedWidgetAsync(AppDbContext db, string name = "Sprocket")
    {
        var widget = new Widget
        {
            Sku = "SPR-001",
            Name = name,
            Description = "A widget",
            Manufacturer = "Acme",
            Weight = 1.0m,
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
        var handler = new GetOrderForProblemReportHandler(db);

        var result = await handler.HandleAsync(new GetOrderForProblemReportQuery(999, 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetOrderForProblemReportNotFound>();
    }

    [Fact]
    public async Task HandleAsync_OrderBelongsToDifferentCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1);
        var handler = new GetOrderForProblemReportHandler(db);

        var result = await handler.HandleAsync(new GetOrderForProblemReportQuery(order.Id, 2), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetOrderForProblemReportNotFound>();
    }

    [Fact]
    public async Task HandleAsync_DraftOrderBelongsToCustomer_ReturnsNotSubmitted()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, status: OrderStatus.Draft);
        var handler = new GetOrderForProblemReportHandler(db);

        var result = await handler.HandleAsync(new GetOrderForProblemReportQuery(order.Id, 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetOrderForProblemReportNotSubmitted>();
    }

    [Fact]
    public async Task HandleAsync_CancelledOrderBelongsToCustomer_ReturnsNotSubmitted()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, status: OrderStatus.Cancelled);
        var handler = new GetOrderForProblemReportHandler(db);

        var result = await handler.HandleAsync(new GetOrderForProblemReportQuery(order.Id, 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<GetOrderForProblemReportNotSubmitted>();
    }

    [Fact]
    public async Task HandleAsync_SubmittedOrderBelongsToCustomer_ReturnsSuccess()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, status: OrderStatus.Submitted);
        var handler = new GetOrderForProblemReportHandler(db);

        var result = await handler.HandleAsync(new GetOrderForProblemReportQuery(order.Id, 1), TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<GetOrderForProblemReportResponse>();
        response.OrderId.ShouldBe(order.Id);
        response.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_SubmittedOrderWithItems_ReturnsWidgetNameAndQuantity()
    {
        using var db = CreateDb();
        var widget = await SeedWidgetAsync(db, "Sprocket");
        var order = await SeedOrderAsync(db, customerId: 1, status: OrderStatus.Submitted);
        db.OrderItems.Add(new OrderItem { OrderId = order.Id, WidgetId = widget.Id, Quantity = 3 });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new GetOrderForProblemReportHandler(db);

        var result = await handler.HandleAsync(new GetOrderForProblemReportQuery(order.Id, 1), TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<GetOrderForProblemReportResponse>();
        response.Items.Count.ShouldBe(1);
        response.Items[0].WidgetName.ShouldBe("Sprocket");
        response.Items[0].Quantity.ShouldBe(3);
    }
}