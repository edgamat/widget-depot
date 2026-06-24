using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.ProblemReports.GetMyProblemReports;

namespace WidgetDepot.Tests.Features.ProblemReports.GetMyProblemReports;

public class GetMyProblemReportsHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Order> SeedSubmittedOrderAsync(AppDbContext db, int customerId = 1, DateTime? submittedAt = null)
    {
        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            SubmittedAt = submittedAt ?? DateTime.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return order;
    }

    private static async Task<ProblemReport> SeedProblemReportAsync(AppDbContext db, int orderId, bool emailSent = false, DateTime? createdAt = null)
    {
        var report = new ProblemReport
        {
            OrderId = orderId,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            EmailSent = emailSent
        };
        db.ProblemReports.Add(report);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return report;
    }

    [Fact]
    public async Task HandleAsync_NoReports_ReturnsEmptyList()
    {
        using var db = CreateDb();
        var handler = new GetMyProblemReportsHandler(db);

        var result = await handler.HandleAsync(new GetMyProblemReportsQuery(1), TestContext.Current.CancellationToken);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ReportsForDifferentCustomer_ReturnsEmptyList()
    {
        using var db = CreateDb();
        var order = await SeedSubmittedOrderAsync(db, customerId: 2);
        await SeedProblemReportAsync(db, order.Id);
        var handler = new GetMyProblemReportsHandler(db);

        var result = await handler.HandleAsync(new GetMyProblemReportsQuery(1), TestContext.Current.CancellationToken);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_CustomerHasReports_ReturnsTheirReports()
    {
        using var db = CreateDb();
        var order = await SeedSubmittedOrderAsync(db, customerId: 1);
        await SeedProblemReportAsync(db, order.Id);
        var handler = new GetMyProblemReportsHandler(db);

        var result = await handler.HandleAsync(new GetMyProblemReportsQuery(1), TestContext.Current.CancellationToken);

        result.Count.ShouldBe(1);
        result[0].OrderId.ShouldBe(order.Id);
    }

    [Fact]
    public async Task HandleAsync_ReturnsCorrectFields()
    {
        using var db = CreateDb();
        var submittedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        var filedAt = new DateTime(2026, 1, 16, 0, 0, 0, DateTimeKind.Utc);
        var order = await SeedSubmittedOrderAsync(db, customerId: 1, submittedAt: submittedAt);
        await SeedProblemReportAsync(db, order.Id, emailSent: true, createdAt: filedAt);
        var handler = new GetMyProblemReportsHandler(db);

        var result = await handler.HandleAsync(new GetMyProblemReportsQuery(1), TestContext.Current.CancellationToken);

        var item = result[0];
        item.OrderId.ShouldBe(order.Id);
        item.OrderSubmittedAt.ShouldBe(submittedAt);
        item.ReportFiledAt.ShouldBe(filedAt);
        item.EmailSent.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAsync_ReturnsAtMostTenReports()
    {
        using var db = CreateDb();
        var order = await SeedSubmittedOrderAsync(db, customerId: 1);
        for (var i = 0; i < 12; i++)
            await SeedProblemReportAsync(db, order.Id);
        var handler = new GetMyProblemReportsHandler(db);

        var result = await handler.HandleAsync(new GetMyProblemReportsQuery(1), TestContext.Current.CancellationToken);

        result.Count.ShouldBe(10);
    }

    [Fact]
    public async Task HandleAsync_ReturnsReportsOrderedByCreatedAtDescending()
    {
        using var db = CreateDb();
        var order = await SeedSubmittedOrderAsync(db, customerId: 1);
        var older = await SeedProblemReportAsync(db, order.Id, createdAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var newer = await SeedProblemReportAsync(db, order.Id, createdAt: new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc));
        var handler = new GetMyProblemReportsHandler(db);

        var result = await handler.HandleAsync(new GetMyProblemReportsQuery(1), TestContext.Current.CancellationToken);

        result[0].Id.ShouldBe(newer.Id);
        result[1].Id.ShouldBe(older.Id);
    }
}