using Microsoft.EntityFrameworkCore;

using Moq;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Orders.Submit;

namespace WidgetDepot.Tests.Features.Orders.Submit;

public class SubmitOrderHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static SubmitOrderHandler CreateHandler(AppDbContext db, IOrderFileWriter? writer = null)
    {
        var mockWriter = writer ?? new Mock<IOrderFileWriter>().Object;
        return new SubmitOrderHandler(db, mockWriter);
    }

    private static async Task<(Order order, Customer customer)> SeedFullOrderAsync(AppDbContext db, int customerId = 1)
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

        var widget = new Widget { Id = 1, Sku = "W-001", Name = "Sprocket", Weight = 2.0m };
        db.Widgets.Add(widget);

        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
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
            },
            ShippingEstimate = 12.99m
        };
        order.Items.Add(new OrderItem { WidgetId = widget.Id, Quantity = 2 });
        db.Orders.Add(order);

        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (order, customer);
    }

    [Fact]
    public async Task HandleAsync_OrderNotFound_ReturnsOrderNotFound()
    {
        using var db = CreateDb();
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(999, customerId: 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitOrderError.OrderNotFound>();
    }

    [Fact]
    public async Task HandleAsync_OrderBelongsToOtherCustomer_ReturnsForbidden()
    {
        using var db = CreateDb();
        var (order, _) = await SeedFullOrderAsync(db, customerId: 2);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitOrderError.Forbidden>();
    }

    [Fact]
    public async Task HandleAsync_OrderAlreadySubmitted_ReturnsInvalidOrderState()
    {
        using var db = CreateDb();
        var (order, _) = await SeedFullOrderAsync(db);
        order.Status = OrderStatus.Submitted;
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitOrderError.InvalidOrderState>();
    }

    [Fact]
    public async Task HandleAsync_OrderHasNoItems_ReturnsIncompleteOrder()
    {
        using var db = CreateDb();
        var customer = new Customer { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        db.Customers.Add(customer);
        var order = new Order
        {
            CustomerId = 1,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            ShippingAddress = new Address { RecipientName = "A", StreetLine1 = "1 St", City = "City", State = "IL", ZipCode = "12345" },
            BillingAddress = new Address { RecipientName = "A", StreetLine1 = "1 St", City = "City", State = "IL", ZipCode = "12345" },
            ShippingEstimate = 9.99m
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitOrderError.IncompleteOrder>();
    }

    [Fact]
    public async Task HandleAsync_MissingShippingAddress_ReturnsIncompleteOrder()
    {
        using var db = CreateDb();
        var customer = new Customer { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        db.Customers.Add(customer);
        var widget = new Widget { Id = 1, Sku = "W-001", Name = "W", Weight = 1m };
        db.Widgets.Add(widget);
        var order = new Order
        {
            CustomerId = 1,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            BillingAddress = new Address { RecipientName = "A", StreetLine1 = "1 St", City = "City", State = "IL", ZipCode = "12345" },
            ShippingEstimate = 9.99m
        };
        order.Items.Add(new OrderItem { WidgetId = widget.Id, Quantity = 1 });
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitOrderError.IncompleteOrder>();
    }

    [Fact]
    public async Task HandleAsync_MissingBillingAddress_ReturnsIncompleteOrder()
    {
        using var db = CreateDb();
        var customer = new Customer { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        db.Customers.Add(customer);
        var widget = new Widget { Id = 1, Sku = "W-001", Name = "W", Weight = 1m };
        db.Widgets.Add(widget);
        var order = new Order
        {
            CustomerId = 1,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            ShippingAddress = new Address { RecipientName = "A", StreetLine1 = "1 St", City = "City", State = "IL", ZipCode = "12345" },
            ShippingEstimate = 9.99m
        };
        order.Items.Add(new OrderItem { WidgetId = widget.Id, Quantity = 1 });
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitOrderError.IncompleteOrder>();
    }

    [Fact]
    public async Task HandleAsync_MissingShippingEstimate_ReturnsIncompleteOrder()
    {
        using var db = CreateDb();
        var customer = new Customer { Id = 1, FirstName = "A", LastName = "B", Email = "a@b.com", PasswordHash = "h", CreatedAt = DateTime.UtcNow };
        db.Customers.Add(customer);
        var widget = new Widget { Id = 1, Sku = "W-001", Name = "W", Weight = 1m };
        db.Widgets.Add(widget);
        var order = new Order
        {
            CustomerId = 1,
            Status = OrderStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            ShippingAddress = new Address { RecipientName = "A", StreetLine1 = "1 St", City = "City", State = "IL", ZipCode = "12345" },
            BillingAddress = new Address { RecipientName = "A", StreetLine1 = "1 St", City = "City", State = "IL", ZipCode = "12345" }
        };
        order.Items.Add(new OrderItem { WidgetId = widget.Id, Quantity = 1 });
        db.Orders.Add(order);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<SubmitOrderError.IncompleteOrder>();
    }

    [Fact]
    public async Task HandleAsync_ValidOrder_ReturnsSubmitOrderResponse()
    {
        using var db = CreateDb();
        var (order, _) = await SeedFullOrderAsync(db);
        var handler = CreateHandler(db);

        var result = await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<SubmitOrderResponse>();
        response.OrderId.ShouldBe(order.Id);
    }

    [Fact]
    public async Task HandleAsync_ValidOrder_SetsOrderStatusToSubmitted()
    {
        using var db = CreateDb();
        var (order, _) = await SeedFullOrderAsync(db);
        var handler = CreateHandler(db);

        await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.Status.ShouldBe(OrderStatus.Submitted);
    }

    [Fact]
    public async Task HandleAsync_ValidOrder_SetsSubmittedAt()
    {
        using var db = CreateDb();
        var (order, _) = await SeedFullOrderAsync(db);
        var handler = CreateHandler(db);

        var before = DateTime.UtcNow;
        await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);
        var after = DateTime.UtcNow;

        var saved = await db.Orders.FirstAsync(TestContext.Current.CancellationToken);
        saved.SubmittedAt.ShouldNotBeNull();
        saved.SubmittedAt.Value.ShouldBeGreaterThanOrEqualTo(before);
        saved.SubmittedAt.Value.ShouldBeLessThanOrEqualTo(after);
    }

    [Fact]
    public async Task HandleAsync_ValidOrder_CallsOrderFileWriter()
    {
        using var db = CreateDb();
        var (order, _) = await SeedFullOrderAsync(db);

        var mockWriter = new Mock<IOrderFileWriter>();
        mockWriter.Setup(w => w.WriteAsync(It.IsAny<OrderFile>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(db, mockWriter.Object);

        await handler.HandleAsync(order.Id, customerId: 1, TestContext.Current.CancellationToken);

        mockWriter.Verify(w => w.WriteAsync(It.IsAny<OrderFile>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}