using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Accounts.ForcePasswordChange;

namespace WidgetDepot.Tests.Features.Accounts.ForcePasswordChange;

public class ForcePasswordChangeHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Customer> SeedCustomerAsync(AppDbContext db, bool mustChangePassword = true)
    {
        var hasher = new PasswordHasher<Customer>();
        var customer = new Customer
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            MustChangePassword = mustChangePassword,
            CreatedAt = DateTime.UtcNow
        };
        customer.PasswordHash = hasher.HashPassword(customer, "TmpPassword1!");
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer;
    }

    [Fact]
    public async Task ChangeAsync_ExistingCustomer_ReturnsSuccess()
    {
        using var db = CreateDb();
        var customer = await SeedCustomerAsync(db);
        var handler = new ForcePasswordChangeHandler(db);
        var request = new ForcePasswordChangeRequest("NewP@ss1!");

        var result = await handler.HandleAsync(new ForcePasswordChangeCommand(customer.Id, request), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ForcePasswordChangeSuccess>();
    }

    [Fact]
    public async Task ChangeAsync_ExistingCustomer_UpdatesPasswordHash()
    {
        using var db = CreateDb();
        var customer = await SeedCustomerAsync(db);
        var originalHash = customer.PasswordHash;
        var handler = new ForcePasswordChangeHandler(db);
        var request = new ForcePasswordChangeRequest("NewP@ss1!");

        await handler.HandleAsync(new ForcePasswordChangeCommand(customer.Id, request), TestContext.Current.CancellationToken);

        var updated = await db.Customers.SingleAsync(TestContext.Current.CancellationToken);
        updated.PasswordHash.ShouldNotBe(originalHash);

        var hasher = new PasswordHasher<Customer>();
        var verify = hasher.VerifyHashedPassword(updated, updated.PasswordHash, "NewP@ss1!");
        verify.ShouldNotBe(PasswordVerificationResult.Failed);
    }

    [Fact]
    public async Task ChangeAsync_ExistingCustomer_ClearsMustChangePasswordFlag()
    {
        using var db = CreateDb();
        var customer = await SeedCustomerAsync(db, mustChangePassword: true);
        var handler = new ForcePasswordChangeHandler(db);
        var request = new ForcePasswordChangeRequest("NewP@ss1!");

        await handler.HandleAsync(new ForcePasswordChangeCommand(customer.Id, request), TestContext.Current.CancellationToken);

        var updated = await db.Customers.SingleAsync(TestContext.Current.CancellationToken);
        updated.MustChangePassword.ShouldBeFalse();
    }

    [Fact]
    public async Task ChangeAsync_UnknownCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = new ForcePasswordChangeHandler(db);
        var request = new ForcePasswordChangeRequest("NewP@ss1!");

        var result = await handler.HandleAsync(new ForcePasswordChangeCommand(99, request), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ForcePasswordChangeError.NotFound>();
    }
}