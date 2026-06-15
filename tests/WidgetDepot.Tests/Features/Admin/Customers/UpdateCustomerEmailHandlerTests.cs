using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Admin.Customers.UpdateCustomerEmail;

namespace WidgetDepot.Tests.Features.Admin.Customers;

public class UpdateCustomerEmailHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Customer SeedCustomer(AppDbContext db, int id = 1, string email = "jane@example.com")
    {
        var customer = new Customer
        {
            Id = id,
            FirstName = "Jane",
            LastName = "Doe",
            Email = email,
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        db.Customers.Add(customer);
        db.SaveChanges();
        return customer;
    }

    [Fact]
    public async Task UpdateAsync_ValidNewEmail_UpdatesEmailAndReturnsSuccess()
    {
        using var db = CreateDb();
        SeedCustomer(db);
        var handler = new UpdateCustomerEmailHandler(db);
        var request = new UpdateCustomerEmailRequest("new@example.com");

        var result = await handler.HandleAsync(new UpdateCustomerEmailCommand(1, request), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateCustomerEmailSuccess>();

        var updated = await db.Customers.SingleAsync(TestContext.Current.CancellationToken);
        updated.Email.ShouldBe("new@example.com");
    }

    [Fact]
    public async Task UpdateAsync_SameEmail_UpdatesEmailAndReturnsSuccess()
    {
        using var db = CreateDb();
        SeedCustomer(db, email: "jane@example.com");
        var handler = new UpdateCustomerEmailHandler(db);
        var request = new UpdateCustomerEmailRequest("jane@example.com");

        var result = await handler.HandleAsync(new UpdateCustomerEmailCommand(1, request), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateCustomerEmailSuccess>();
    }

    [Fact]
    public async Task UpdateAsync_EmailInUseByAnotherCustomer_ReturnsEmailAlreadyInUse()
    {
        using var db = CreateDb();
        SeedCustomer(db, id: 1, email: "jane@example.com");
        SeedCustomer(db, id: 2, email: "other@example.com");
        var handler = new UpdateCustomerEmailHandler(db);
        var request = new UpdateCustomerEmailRequest("other@example.com");

        var result = await handler.HandleAsync(new UpdateCustomerEmailCommand(1, request), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateCustomerEmailError.EmailAlreadyInUse>();
    }

    [Fact]
    public async Task UpdateAsync_UnknownCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = new UpdateCustomerEmailHandler(db);
        var request = new UpdateCustomerEmailRequest("new@example.com");

        var result = await handler.HandleAsync(new UpdateCustomerEmailCommand(99, request), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateCustomerEmailError.NotFound>();
    }
}