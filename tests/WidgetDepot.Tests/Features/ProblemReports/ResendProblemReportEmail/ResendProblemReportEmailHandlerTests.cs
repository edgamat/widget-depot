using Microsoft.EntityFrameworkCore;

using Moq;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.ProblemReports.Email;
using WidgetDepot.ApiService.Features.ProblemReports.ResendProblemReportEmail;

namespace WidgetDepot.Tests.Features.ProblemReports.ResendProblemReportEmail;

public class ResendProblemReportEmailHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Mock<IProblemReportEmailSender> CreateEmailSender(bool returns = true)
    {
        var mock = new Mock<IProblemReportEmailSender>();
        mock.Setup(s => s.SendAsync(It.IsAny<ProblemReportEmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(returns);
        return mock;
    }

    private static async Task<(Order Order, OrderItem Item, Widget Widget)> SeedSubmittedOrderWithItemAsync(
        AppDbContext db, int customerId = 1)
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

        return (order, item, widget);
    }

    private static async Task<ProblemReport> SeedProblemReportAsync(
        AppDbContext db, int orderId, int orderItemId, bool emailSent = false)
    {
        var report = new ProblemReport
        {
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow,
            EmailSent = emailSent,
            Items = [new ProblemReportItem { OrderItemId = orderItemId, IssueType = IssueType.Damaged }]
        };
        db.ProblemReports.Add(report);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return report;
    }

    [Fact]
    public async Task HandleAsync_ReportNotFound_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = new ResendProblemReportEmailHandler(db, CreateEmailSender().Object);

        var result = await handler.HandleAsync(new ResendProblemReportEmailCommand(999, 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ResendProblemReportEmailNotFound>();
    }

    [Fact]
    public async Task HandleAsync_ReportBelongsToDifferentCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var (order, item, _) = await SeedSubmittedOrderWithItemAsync(db, customerId: 2);
        var report = await SeedProblemReportAsync(db, order.Id, item.Id);
        var handler = new ResendProblemReportEmailHandler(db, CreateEmailSender().Object);

        var result = await handler.HandleAsync(new ResendProblemReportEmailCommand(report.Id, 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ResendProblemReportEmailNotFound>();
    }

    [Fact]
    public async Task HandleAsync_EmailSendSucceeds_ReturnsSuccess()
    {
        using var db = CreateDb();
        var (order, item, _) = await SeedSubmittedOrderWithItemAsync(db);
        var report = await SeedProblemReportAsync(db, order.Id, item.Id);
        var handler = new ResendProblemReportEmailHandler(db, CreateEmailSender(returns: true).Object);

        var result = await handler.HandleAsync(new ResendProblemReportEmailCommand(report.Id, 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ResendProblemReportEmailSuccess>();
    }

    [Fact]
    public async Task HandleAsync_EmailSendSucceeds_SetsEmailSentToTrue()
    {
        using var db = CreateDb();
        var (order, item, _) = await SeedSubmittedOrderWithItemAsync(db);
        var report = await SeedProblemReportAsync(db, order.Id, item.Id, emailSent: false);
        var handler = new ResendProblemReportEmailHandler(db, CreateEmailSender(returns: true).Object);

        await handler.HandleAsync(new ResendProblemReportEmailCommand(report.Id, 1), TestContext.Current.CancellationToken);

        var saved = await db.ProblemReports.FirstAsync(TestContext.Current.CancellationToken);
        saved.EmailSent.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAsync_EmailSendFails_ReturnsFailed()
    {
        using var db = CreateDb();
        var (order, item, _) = await SeedSubmittedOrderWithItemAsync(db);
        var report = await SeedProblemReportAsync(db, order.Id, item.Id);
        var handler = new ResendProblemReportEmailHandler(db, CreateEmailSender(returns: false).Object);

        var result = await handler.HandleAsync(new ResendProblemReportEmailCommand(report.Id, 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ResendProblemReportEmailFailed>();
    }

    [Fact]
    public async Task HandleAsync_EmailSendFails_EmailSentRemainsUnchanged()
    {
        using var db = CreateDb();
        var (order, item, _) = await SeedSubmittedOrderWithItemAsync(db);
        var report = await SeedProblemReportAsync(db, order.Id, item.Id, emailSent: false);
        var handler = new ResendProblemReportEmailHandler(db, CreateEmailSender(returns: false).Object);

        await handler.HandleAsync(new ResendProblemReportEmailCommand(report.Id, 1), TestContext.Current.CancellationToken);

        var saved = await db.ProblemReports.FirstAsync(TestContext.Current.CancellationToken);
        saved.EmailSent.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_EmailSendSucceeds_SendsEmailWithCorrectOrderId()
    {
        using var db = CreateDb();
        var (order, item, widget) = await SeedSubmittedOrderWithItemAsync(db);
        var report = await SeedProblemReportAsync(db, order.Id, item.Id);
        var emailSender = CreateEmailSender();
        var handler = new ResendProblemReportEmailHandler(db, emailSender.Object);

        await handler.HandleAsync(new ResendProblemReportEmailCommand(report.Id, 1), TestContext.Current.CancellationToken);

        emailSender.Verify(s => s.SendAsync(
            It.Is<ProblemReportEmailMessage>(m =>
                m.OrderId == order.Id &&
                m.Items.Count == 1 &&
                m.Items[0].WidgetName == widget.Name &&
                m.Items[0].IssueType == "Damaged"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}