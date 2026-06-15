using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders;
using WidgetDepot.ApiService.Features.Orders.TransmitOrders;

namespace WidgetDepot.Tests.Features.Orders.TransmitOrders;

public class TransmitOrdersHandlerTests : IDisposable
{
    private readonly string _tempDir;

    public TransmitOrdersHandlerTests()
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

    private TransmitOrdersHandler CreateHandler(
        AppDbContext db,
        IOrderTransmitter? transmitter = null,
        string? pickupDirectory = null)
    {
        var mockTransmitter = transmitter ?? new Mock<IOrderTransmitter>().Object;
        var options = Options.Create(new OrdersOptions { PickupDirectory = pickupDirectory ?? _tempDir });
        var logger = new Mock<ILogger<TransmitOrdersHandler>>().Object;
        return new TransmitOrdersHandler(db, mockTransmitter, options, logger);
    }

    private static async Task<Order> SeedOrderAsync(AppDbContext db, TransmissionStatus? transmissionStatus)
    {
        var order = new Order
        {
            CustomerId = 1,
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
    public async Task HandleAsync_NoPendingOrders_ReturnsZero()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(new TransmitOrdersCommand(), TestContext.Current.CancellationToken);

        result.ShouldBe(0);
    }

    [Fact]
    public async Task HandleAsync_FileNotFound_SetsStatusToMissing()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, TransmissionStatus.Pending);
        var handler = CreateHandler(db);

        await handler.HandleAsync(new TransmitOrdersCommand(), TestContext.Current.CancellationToken);

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatus.ShouldBe(TransmissionStatus.Missing);
    }

    [Fact]
    public async Task HandleAsync_FileNotFound_SetsTransmissionStatusChangedAt()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, TransmissionStatus.Pending);
        var handler = CreateHandler(db);

        var before = DateTime.UtcNow;
        await handler.HandleAsync(new TransmitOrdersCommand(), TestContext.Current.CancellationToken);
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
        var order = await SeedOrderAsync(db, TransmissionStatus.Pending);
        CreateOrderFile(order.Id);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter
            .Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(db, transmitter.Object);

        await handler.HandleAsync(new TransmitOrdersCommand(), TestContext.Current.CancellationToken);

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatus.ShouldBe(TransmissionStatus.Transmitted);
    }

    [Fact]
    public async Task HandleAsync_TransmissionSucceeds_SetsTransmissionStatusChangedAt()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, TransmissionStatus.Pending);
        CreateOrderFile(order.Id);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter
            .Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(db, transmitter.Object);

        var before = DateTime.UtcNow;
        await handler.HandleAsync(new TransmitOrdersCommand(), TestContext.Current.CancellationToken);
        var after = DateTime.UtcNow;

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatusChangedAt.ShouldNotBeNull();
        saved.TransmissionStatusChangedAt.Value.ShouldBeGreaterThanOrEqualTo(before);
        saved.TransmissionStatusChangedAt.Value.ShouldBeLessThanOrEqualTo(after);
    }

    [Fact]
    public async Task HandleAsync_TransmissionFails_SetsStatusToFailed()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, TransmissionStatus.Pending);
        CreateOrderFile(order.Id);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter
            .Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = CreateHandler(db, transmitter.Object);

        await handler.HandleAsync(new TransmitOrdersCommand(), TestContext.Current.CancellationToken);

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatus.ShouldBe(TransmissionStatus.Failed);
    }

    [Fact]
    public async Task HandleAsync_TransmissionThrows_SetsStatusToFailed()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, TransmissionStatus.Pending);
        CreateOrderFile(order.Id);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter
            .Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("FTP error"));

        var handler = CreateHandler(db, transmitter.Object);

        await handler.HandleAsync(new TransmitOrdersCommand(), TestContext.Current.CancellationToken);

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatus.ShouldBe(TransmissionStatus.Failed);
    }

    [Fact]
    public async Task HandleAsync_OrderWithFailedStatus_IsNotProcessed()
    {
        using var db = CreateDb();
        await SeedOrderAsync(db, TransmissionStatus.Failed);

        var transmitter = new Mock<IOrderTransmitter>();
        var handler = CreateHandler(db, transmitter.Object);

        var result = await handler.HandleAsync(new TransmitOrdersCommand(), TestContext.Current.CancellationToken);

        result.ShouldBe(0);
        transmitter.Verify(
            t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_OrderWithMissingStatus_IsRetried()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, TransmissionStatus.Missing);
        CreateOrderFile(order.Id);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter
            .Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(db, transmitter.Object);

        await handler.HandleAsync(new TransmitOrdersCommand(), TestContext.Current.CancellationToken);

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatus.ShouldBe(TransmissionStatus.Transmitted);
    }

    [Fact]
    public async Task HandleAsync_TransmissionSucceeds_CallsTransmitterWithCorrectFileName()
    {
        using var db = CreateDb();
        var order = await SeedOrderAsync(db, TransmissionStatus.Pending);
        CreateOrderFile(order.Id);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter
            .Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(db, transmitter.Object);

        await handler.HandleAsync(new TransmitOrdersCommand(), TestContext.Current.CancellationToken);

        var expectedFileName = $"EXT-{order.Id.ToString().PadLeft(10, '0')}.TXT";
        transmitter.Verify(
            t => t.TransmitAsync(It.IsAny<string>(), expectedFileName, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}