using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders;
using WidgetDepot.ApiService.Features.Orders.RetransmitOrder;
using WidgetDepot.ApiService.Features.Orders.TransmitOrders;

namespace WidgetDepot.Tests.Features.Orders.RetransmitOrder;

public class RetransmitOrderHandlerTests : IDisposable
{
    private readonly string _tempDir;

    public RetransmitOrderHandlerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        Directory.Delete(_tempDir, recursive: true);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private RetransmitOrderHandler CreateHandler(
        AppDbContext db,
        IOrderTransmitter? transmitter = null,
        string? pickupDirectory = null)
    {
        var mockTransmitter = transmitter ?? new Mock<IOrderTransmitter>().Object;
        var options = Options.Create(new OrdersOptions { PickupDirectory = pickupDirectory ?? _tempDir });
        var logger = new Mock<ILogger<RetransmitOrderHandler>>().Object;
        return new RetransmitOrderHandler(db, mockTransmitter, options, logger);
    }

    private static async Task<Order> SeedOrderAsync(AppDbContext db, int customerId, TransmissionStatus? transmissionStatus)
    {
        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            TransmissionStatus = transmissionStatus
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return order;
    }

    private string CreateOrderFile(int orderId)
    {
        var fileName = $"EXT-{orderId.ToString().PadLeft(10, '0')}.TXT";
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, "test content");
        return path;
    }

    [Fact]
    public async Task HandleAsync_OrderNotFound_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new RetransmitOrderCommand(999, 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RetransmitOrderNotFound>();
    }

    [Fact]
    public async Task HandleAsync_OrderBelongsToDifferentCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, TransmissionStatus.Failed);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new RetransmitOrderCommand(order.Id, CustomerId: 2), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RetransmitOrderNotFound>();
    }

    [Fact]
    public async Task HandleAsync_OrderStatusIsPending_ReturnsInvalidStatus()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, TransmissionStatus.Pending);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new RetransmitOrderCommand(order.Id, CustomerId: 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RetransmitOrderInvalidStatus>();
    }

    [Fact]
    public async Task HandleAsync_OrderStatusIsTransmitted_ReturnsInvalidStatus()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, TransmissionStatus.Transmitted);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new RetransmitOrderCommand(order.Id, CustomerId: 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RetransmitOrderInvalidStatus>();
    }

    [Fact]
    public async Task HandleAsync_OrderStatusIsMissing_ReturnsInvalidStatus()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, TransmissionStatus.Missing);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new RetransmitOrderCommand(order.Id, CustomerId: 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RetransmitOrderInvalidStatus>();
    }

    [Fact]
    public async Task HandleAsync_FileNotFound_SetsStatusToMissing()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, TransmissionStatus.Failed);
        var handler = CreateHandler(db);

        await handler.HandleAsync(new RetransmitOrderCommand(order.Id, CustomerId: 1), TestContext.Current.CancellationToken);

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatus.ShouldBe(TransmissionStatus.Missing);
    }

    [Fact]
    public async Task HandleAsync_FileNotFound_ReturnsResponseWithMissingStatus()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, TransmissionStatus.Failed);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new RetransmitOrderCommand(order.Id, CustomerId: 1), TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<RetransmitOrderResponse>();
        response.NewStatus.ShouldBe(TransmissionStatus.Missing);
    }

    [Fact]
    public async Task HandleAsync_FileNotFound_SetsTransmissionStatusChangedAt()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, TransmissionStatus.Failed);
        var handler = CreateHandler(db);

        var before = DateTime.UtcNow;
        await handler.HandleAsync(new RetransmitOrderCommand(order.Id, CustomerId: 1), TestContext.Current.CancellationToken);
        var after = DateTime.UtcNow;

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatusChangedAt.ShouldNotBeNull();
        saved.TransmissionStatusChangedAt.Value.ShouldBeGreaterThanOrEqualTo(before);
        saved.TransmissionStatusChangedAt.Value.ShouldBeLessThanOrEqualTo(after);
    }

    [Fact]
    public async Task HandleAsync_TransmissionSucceeds_SetsStatusToTransmitted()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, TransmissionStatus.Failed);
        CreateOrderFile(order.Id);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter
            .Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(db, transmitter.Object);

        await handler.HandleAsync(new RetransmitOrderCommand(order.Id, CustomerId: 1), TestContext.Current.CancellationToken);

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatus.ShouldBe(TransmissionStatus.Transmitted);
    }

    [Fact]
    public async Task HandleAsync_TransmissionSucceeds_ReturnsResponseWithTransmittedStatus()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, TransmissionStatus.Failed);
        CreateOrderFile(order.Id);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter
            .Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(db, transmitter.Object);

        var result = await handler.HandleAsync(new RetransmitOrderCommand(order.Id, CustomerId: 1), TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<RetransmitOrderResponse>();
        response.NewStatus.ShouldBe(TransmissionStatus.Transmitted);
    }

    [Fact]
    public async Task HandleAsync_TransmissionSucceeds_SetsTransmissionStatusChangedAt()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, TransmissionStatus.Failed);
        CreateOrderFile(order.Id);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter
            .Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(db, transmitter.Object);

        var before = DateTime.UtcNow;
        await handler.HandleAsync(new RetransmitOrderCommand(order.Id, CustomerId: 1), TestContext.Current.CancellationToken);
        var after = DateTime.UtcNow;

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatusChangedAt.ShouldNotBeNull();
        saved.TransmissionStatusChangedAt.Value.ShouldBeGreaterThanOrEqualTo(before);
        saved.TransmissionStatusChangedAt.Value.ShouldBeLessThanOrEqualTo(after);
    }

    [Fact]
    public async Task HandleAsync_TransmissionFails_StatusRemainsFailedWithUpdatedTimestamp()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, TransmissionStatus.Failed);
        CreateOrderFile(order.Id);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter
            .Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = CreateHandler(db, transmitter.Object);

        var before = DateTime.UtcNow;
        await handler.HandleAsync(new RetransmitOrderCommand(order.Id, CustomerId: 1), TestContext.Current.CancellationToken);
        var after = DateTime.UtcNow;

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatus.ShouldBe(TransmissionStatus.Failed);
        saved.TransmissionStatusChangedAt.ShouldNotBeNull();
        saved.TransmissionStatusChangedAt.Value.ShouldBeGreaterThanOrEqualTo(before);
        saved.TransmissionStatusChangedAt.Value.ShouldBeLessThanOrEqualTo(after);
    }

    [Fact]
    public async Task HandleAsync_TransmissionThrows_StatusRemainsFailedWithUpdatedTimestamp()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, TransmissionStatus.Failed);
        CreateOrderFile(order.Id);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter
            .Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("FTP error"));

        var handler = CreateHandler(db, transmitter.Object);

        var before = DateTime.UtcNow;
        await handler.HandleAsync(new RetransmitOrderCommand(order.Id, CustomerId: 1), TestContext.Current.CancellationToken);
        var after = DateTime.UtcNow;

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatus.ShouldBe(TransmissionStatus.Failed);
        saved.TransmissionStatusChangedAt.ShouldNotBeNull();
        saved.TransmissionStatusChangedAt.Value.ShouldBeGreaterThanOrEqualTo(before);
        saved.TransmissionStatusChangedAt.Value.ShouldBeLessThanOrEqualTo(after);
    }

    [Fact]
    public async Task HandleAsync_TransmissionSucceeds_CallsTransmitterWithCorrectFileName()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, customerId: 1, TransmissionStatus.Failed);
        CreateOrderFile(order.Id);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter
            .Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(db, transmitter.Object);

        await handler.HandleAsync(new RetransmitOrderCommand(order.Id, CustomerId: 1), TestContext.Current.CancellationToken);

        var expectedFileName = $"EXT-{order.Id.ToString().PadLeft(10, '0')}.TXT";
        transmitter.Verify(
            t => t.TransmitAsync(It.IsAny<string>(), expectedFileName, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}