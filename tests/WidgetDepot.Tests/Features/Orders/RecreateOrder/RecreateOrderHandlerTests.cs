using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders;
using WidgetDepot.ApiService.Features.Orders.RecreateOrder;
using WidgetDepot.ApiService.Features.Orders.Submit;
using WidgetDepot.ApiService.Features.Orders.TransmitOrders;

namespace WidgetDepot.Tests.Features.Orders.RecreateOrder;

public class RecreateOrderHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static RecreateOrderHandler CreateHandler(
        AppDbContext db,
        IOrderFileWriter? writer = null,
        IOrderTransmitter? transmitter = null)
    {
        var mockWriter = writer ?? new Mock<IOrderFileWriter>().Object;
        var mockTransmitter = transmitter ?? new Mock<IOrderTransmitter>().Object;
        var options = Options.Create(new OrdersOptions { PickupDirectory = "/tmp/orders" });
        var logger = new Mock<ILogger<RecreateOrderHandler>>().Object;
        return new RecreateOrderHandler(db, mockWriter, mockTransmitter, options, logger);
    }

    private static async Task<(Order order, Customer customer)> SeedMissingOrderAsync(AppDbContext db, int customerId = 1)
    {
        var customer = new Customer
        {
            Id = customerId,
            FirstName = "Alice",
            LastName = "Smith",
            Email = "alice@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        db.Customers.Add(customer);

        var widget = new Widget { Id = 1, Sku = "W-001", Name = "Sprocket", Weight = 1.5m };
        db.Widgets.Add(widget);

        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Submitted,
            CreatedAt = DateTime.UtcNow,
            SubmittedAt = DateTime.UtcNow,
            TransmissionStatus = TransmissionStatus.Missing,
            ShippingAddress = new Address
            {
                RecipientName = "Alice Smith",
                StreetLine1 = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701"
            },
            BillingAddress = new Address
            {
                RecipientName = "Alice Smith",
                StreetLine1 = "456 Oak Ave",
                City = "Shelbyville",
                State = "IL",
                ZipCode = "62565"
            }
        };
        order.Items.Add(new OrderItem { WidgetId = widget.Id, Quantity = 2 });
        db.Orders.Add(order);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (order, customer);
    }

    [Fact]
    public async Task HandleAsync_OrderNotFound_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(999, customerId: 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RecreateOrderNotFound>();
    }

    [Fact]
    public async Task HandleAsync_OrderBelongsToDifferentCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var (order, _) = await SeedMissingOrderAsync(db, customerId: 1);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(order.Id, customerId: 2, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RecreateOrderNotFound>();
    }

    [Fact]
    public async Task HandleAsync_OrderStatusIsPending_ReturnsInvalidStatus()
    {
        using var db = CreateDb();
        var (order, _) = await SeedMissingOrderAsync(db);
        order.TransmissionStatus = TransmissionStatus.Pending;
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RecreateOrderInvalidStatus>();
    }

    [Fact]
    public async Task HandleAsync_OrderStatusIsTransmitted_ReturnsInvalidStatus()
    {
        using var db = CreateDb();
        var (order, _) = await SeedMissingOrderAsync(db);
        order.TransmissionStatus = TransmissionStatus.Transmitted;
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RecreateOrderInvalidStatus>();
    }

    [Fact]
    public async Task HandleAsync_OrderStatusIsFailed_ReturnsInvalidStatus()
    {
        using var db = CreateDb();
        var (order, _) = await SeedMissingOrderAsync(db);
        order.TransmissionStatus = TransmissionStatus.Failed;
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RecreateOrderInvalidStatus>();
    }

    [Fact]
    public async Task HandleAsync_TransmissionSucceeds_SetsStatusToTransmitted()
    {
        using var db = CreateDb();
        var (order, _) = await SeedMissingOrderAsync(db);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter.Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(db, transmitter: transmitter.Object);

        await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatus.ShouldBe(TransmissionStatus.Transmitted);
    }

    [Fact]
    public async Task HandleAsync_TransmissionSucceeds_ReturnsResponseWithTransmittedStatus()
    {
        using var db = CreateDb();
        var (order, _) = await SeedMissingOrderAsync(db);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter.Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(db, transmitter: transmitter.Object);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<RecreateOrderResponse>();
        response.NewStatus.ShouldBe(TransmissionStatus.Transmitted);
        response.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public async Task HandleAsync_TransmissionSucceeds_SetsTransmissionStatusChangedAt()
    {
        using var db = CreateDb();
        var (order, _) = await SeedMissingOrderAsync(db);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter.Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(db, transmitter: transmitter.Object);

        var before = DateTime.UtcNow;
        await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);
        var after = DateTime.UtcNow;

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatusChangedAt.ShouldNotBeNull();
        saved.TransmissionStatusChangedAt.Value.ShouldBeGreaterThanOrEqualTo(before);
        saved.TransmissionStatusChangedAt.Value.ShouldBeLessThanOrEqualTo(after);
    }

    [Fact]
    public async Task HandleAsync_TransmissionSucceeds_CallsFileWriter()
    {
        using var db = CreateDb();
        var (order, _) = await SeedMissingOrderAsync(db);

        var writer = new Mock<IOrderFileWriter>();
        var transmitter = new Mock<IOrderTransmitter>();
        transmitter.Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = CreateHandler(db, writer.Object, transmitter.Object);

        await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        writer.Verify(w => w.WriteAsync(It.IsAny<OrderFile>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_TransmissionFails_SetsStatusToFailed()
    {
        using var db = CreateDb();
        var (order, _) = await SeedMissingOrderAsync(db);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter.Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = CreateHandler(db, transmitter: transmitter.Object);

        await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatus.ShouldBe(TransmissionStatus.Failed);
    }

    [Fact]
    public async Task HandleAsync_TransmissionFails_ReturnsResponseWithFailedStatusAndErrorMessage()
    {
        using var db = CreateDb();
        var (order, _) = await SeedMissingOrderAsync(db);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter.Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = CreateHandler(db, transmitter: transmitter.Object);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<RecreateOrderResponse>();
        response.NewStatus.ShouldBe(TransmissionStatus.Failed);
        response.ErrorMessage.ShouldNotBeNull();
    }

    [Fact]
    public async Task HandleAsync_TransmissionThrows_SetsStatusToFailed()
    {
        using var db = CreateDb();
        var (order, _) = await SeedMissingOrderAsync(db);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter.Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("FTP error"));

        var handler = CreateHandler(db, transmitter: transmitter.Object);

        await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.TransmissionStatus.ShouldBe(TransmissionStatus.Failed);
    }

    [Fact]
    public async Task HandleAsync_TransmissionThrows_ReturnsResponseWithErrorMessage()
    {
        using var db = CreateDb();
        var (order, _) = await SeedMissingOrderAsync(db);

        var transmitter = new Mock<IOrderTransmitter>();
        transmitter.Setup(t => t.TransmitAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("FTP error"));

        var handler = CreateHandler(db, transmitter: transmitter.Object);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<RecreateOrderResponse>();
        response.NewStatus.ShouldBe(TransmissionStatus.Failed);
        response.ErrorMessage.ShouldBe("FTP error");
    }
}