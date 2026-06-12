using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Admin.Customers.ResetCustomerPassword;

namespace WidgetDepot.Tests.Features.Admin.Customers;

public class ResetCustomerPasswordHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Customer SeedCustomer(AppDbContext db)
    {
        var customer = new Customer
        {
            Id = 1,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            PasswordHash = "original-hash",
            CreatedAt = DateTime.UtcNow
        };
        db.Customers.Add(customer);
        db.SaveChanges();
        return customer;
    }

    [Fact]
    public async Task ResetAsync_ExistingCustomer_ReturnsSuccessWithTemporaryPassword()
    {
        using var db = CreateDb();
        SeedCustomer(db);
        var handler = new ResetCustomerPasswordHandler(db);

        var result = await handler.ResetAsync(1, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<ResetCustomerPasswordSuccess>();
        success.TemporaryPassword.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task ResetAsync_ExistingCustomer_UpdatesPasswordHash()
    {
        using var db = CreateDb();
        SeedCustomer(db);
        var handler = new ResetCustomerPasswordHandler(db);

        var result = await handler.ResetAsync(1, TestContext.Current.CancellationToken);

        var success = result.ShouldBeOfType<ResetCustomerPasswordSuccess>();

        var updated = await db.Customers.SingleAsync(TestContext.Current.CancellationToken);
        updated.PasswordHash.ShouldNotBe("original-hash");

        var hasher = new PasswordHasher<Customer>();
        var verifyResult = hasher.VerifyHashedPassword(updated, updated.PasswordHash, success.TemporaryPassword);
        verifyResult.ShouldNotBe(PasswordVerificationResult.Failed);
    }

    [Fact]
    public async Task ResetAsync_UnknownCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = new ResetCustomerPasswordHandler(db);

        var result = await handler.ResetAsync(99, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ResetCustomerPasswordError.NotFound>();
    }
}