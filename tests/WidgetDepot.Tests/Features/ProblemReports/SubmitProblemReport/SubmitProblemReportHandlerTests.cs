using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.ProblemReports.SubmitProblemReport;

namespace WidgetDepot.Tests.Features.ProblemReports.SubmitProblemReport;

public class SubmitProblemReportHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<(Order Order, OrderItem Item)> SeedSubmittedOrderWithItemAsync(
        AppDbContext db,
        int customerId = 1)
    {
        var widget = new Widget
        {
            Sku = $"SKU-{Guid.NewGuid()}",
            Name = "Sprocket",
            Description = "A widget",
            Manufacturer = "Acme",
            Weight = 1.0m,
            Price = 9.99m,
            DateAvailable = new DateOnly(2026, 1, 1)
        };
        db.Widgets.Add(widget);

        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            SubmittedAt = DateTime.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var item = new OrderItem { OrderId = order.Id, WidgetId = widget.Id, Quantity = 2 };
        db.OrderItems.Add(item);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        return (order, item);
    }

    private static SubmitProblemReportRequest BuildRequest(
        int orderId,
        int customerId,
        IReadOnlyList<SubmitProblemReportItemRequest>? items = null,
        string? notes = null) =>
        new(orderId, customerId, items ?? [], notes);

    [Fact]
    public async Task HandleAsync_OrderNotFound_ReturnsOrderNotFound()
    {
        using var db = CreateDb();
        var handler = new SubmitProblemReportHandler(db);

        var result = await handler.HandleAsync(BuildRequest(999, 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitProblemReportOrderNotFound>();
    }

    [Fact]
    public async Task HandleAsync_OrderBelongsToDifferentCustomer_ReturnsOrderNotFound()
    {
        using var db = CreateDb();
        var (order, _) = await SeedSubmittedOrderWithItemAsync(db, customerId: 1);
        var handler = new SubmitProblemReportHandler(db);

        var result = await handler.HandleAsync(BuildRequest(order.Id, 2), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitProblemReportOrderNotFound>();
    }

    [Fact]
    public async Task HandleAsync_OrderNotSubmitted_ReturnsOrderNotSubmitted()
    {
        using var db = CreateDb();
        var order = new Order { CustomerId = 1, Status = OrderStatus.Draft, CreatedAt = DateTime.UtcNow };
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new SubmitProblemReportHandler(db);

        var result = await handler.HandleAsync(BuildRequest(order.Id, 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitProblemReportOrderNotSubmitted>();
    }

    [Fact]
    public async Task HandleAsync_ItemNotInOrder_ReturnsInvalidItems()
    {
        using var db = CreateDb();
        var (order, _) = await SeedSubmittedOrderWithItemAsync(db, customerId: 1);
        var handler = new SubmitProblemReportHandler(db);
        var items = new List<SubmitProblemReportItemRequest>
        {
            new(99999, "Damaged")
        };

        var result = await handler.HandleAsync(BuildRequest(order.Id, 1, items), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitProblemReportInvalidItems>();
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_SavesProblemReport()
    {
        using var db = CreateDb();
        var (order, item) = await SeedSubmittedOrderWithItemAsync(db, customerId: 1);
        var handler = new SubmitProblemReportHandler(db);
        var items = new List<SubmitProblemReportItemRequest>
        {
            new(item.Id, "Damaged")
        };

        var result = await handler.HandleAsync(BuildRequest(order.Id, 1, items, "Some notes"), TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<SubmitProblemReportResponse>();
        response.ProblemReportId.ShouldBeGreaterThan(0);

        var saved = await db.ProblemReports.Include(pr => pr.Items).FirstAsync(TestContext.Current.CancellationToken);
        saved.OrderId.ShouldBe(order.Id);
        saved.Notes.ShouldBe("Some notes");
        saved.Items.Count.ShouldBe(1);
        saved.Items.First().OrderItemId.ShouldBe(item.Id);
        saved.Items.First().IssueType.ShouldBe(IssueType.Damaged);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_MultipleProblemReportsAllowed()
    {
        using var db = CreateDb();
        var (order, item) = await SeedSubmittedOrderWithItemAsync(db, customerId: 1);
        var handler = new SubmitProblemReportHandler(db);
        var items = new List<SubmitProblemReportItemRequest> { new(item.Id, "Damaged") };

        await handler.HandleAsync(BuildRequest(order.Id, 1, items), TestContext.Current.CancellationToken);
        var result = await handler.HandleAsync(BuildRequest(order.Id, 1, items), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitProblemReportResponse>();
        (await db.ProblemReports.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(2);
    }

    [Fact]
    public async Task HandleAsync_ValidRequest_SetsCreatedAt()
    {
        using var db = CreateDb();
        var (order, item) = await SeedSubmittedOrderWithItemAsync(db, customerId: 1);
        var handler = new SubmitProblemReportHandler(db);
        var items = new List<SubmitProblemReportItemRequest> { new(item.Id, "UnderRequested") };

        await handler.HandleAsync(BuildRequest(order.Id, 1, items), TestContext.Current.CancellationToken);

        var saved = await db.ProblemReports.FirstAsync(TestContext.Current.CancellationToken);
        saved.CreatedAt.ShouldNotBe(default);
    }
}