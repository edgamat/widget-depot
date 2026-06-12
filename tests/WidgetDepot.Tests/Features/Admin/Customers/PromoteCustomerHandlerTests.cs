using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Admin.Customers.PromoteCustomer;

namespace WidgetDepot.Tests.Features.Admin.Customers;

public class PromoteCustomerHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Customer SeedCustomer(AppDbContext db, bool isAdmin = false)
    {
        var customer = new Customer
        {
            Id = 1,
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
    public async Task PromoteAsync_ExistingCustomer_SetsIsAdminTrueAndReturnsSuccess()
    {
        using var db = CreateDb();
        SeedCustomer(db, isAdmin: false);
        var handler = new PromoteCustomerHandler(db);

        var result = await handler.PromoteAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<PromoteCustomerSuccess>();

        var updated = await db.Customers.SingleAsync(TestContext.Current.CancellationToken);
        updated.IsAdmin.ShouldBeTrue();
    }

    [Fact]
    public async Task PromoteAsync_AlreadyAdmin_SetsIsAdminTrueAndReturnsSuccess()
    {
        using var db = CreateDb();
        SeedCustomer(db, isAdmin: true);
        var handler = new PromoteCustomerHandler(db);

        var result = await handler.PromoteAsync(1, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<PromoteCustomerSuccess>();
    }

    [Fact]
    public async Task PromoteAsync_UnknownCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = new PromoteCustomerHandler(db);

        var result = await handler.PromoteAsync(99, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<PromoteCustomerError.NotFound>();
    }
}