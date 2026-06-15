using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Admin.Customers.DemoteCustomer;

namespace WidgetDepot.Tests.Features.Admin.Customers;

public class DemoteCustomerHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Customer SeedCustomer(AppDbContext db, int id = 1, bool isAdmin = true)
    {
        var customer = new Customer
        {
            Id = id,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow,
            IsAdmin = isAdmin
        };
        db.Customers.Add(customer);
        db.SaveChanges();
        return customer;
    }

    [Fact]
    public async Task DemoteAsync_ExistingAdmin_SetsIsAdminFalseAndReturnsSuccess()
    {
        using var db = CreateDb();
        SeedCustomer(db, id: 1, isAdmin: true);
        var handler = new DemoteCustomerHandler(db);

        var result = await handler.HandleAsync(new DemoteCustomerCommand(CustomerId: 1, RequestingAdminId: 2), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<DemoteCustomerSuccess>();

        var updated = await db.Customers.SingleAsync(TestContext.Current.CancellationToken);
        updated.IsAdmin.ShouldBeFalse();
    }

    [Fact]
    public async Task DemoteAsync_SameIdAsRequestingAdmin_ReturnsCannotDemoteSelf()
    {
        using var db = CreateDb();
        SeedCustomer(db, id: 1, isAdmin: true);
        var handler = new DemoteCustomerHandler(db);

        var result = await handler.HandleAsync(new DemoteCustomerCommand(CustomerId: 1, RequestingAdminId: 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<DemoteCustomerError.CannotDemoteSelf>();
    }

    [Fact]
    public async Task DemoteAsync_SameIdAsRequestingAdmin_DoesNotModifyDatabase()
    {
        using var db = CreateDb();
        SeedCustomer(db, id: 1, isAdmin: true);
        var handler = new DemoteCustomerHandler(db);

        await handler.HandleAsync(new DemoteCustomerCommand(CustomerId: 1, RequestingAdminId: 1), TestContext.Current.CancellationToken);

        var customer = await db.Customers.SingleAsync(TestContext.Current.CancellationToken);
        customer.IsAdmin.ShouldBeTrue();
    }

    [Fact]
    public async Task DemoteAsync_UnknownCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = new DemoteCustomerHandler(db);

        var result = await handler.HandleAsync(new DemoteCustomerCommand(CustomerId: 99, RequestingAdminId: 1), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<DemoteCustomerError.NotFound>();
    }
}