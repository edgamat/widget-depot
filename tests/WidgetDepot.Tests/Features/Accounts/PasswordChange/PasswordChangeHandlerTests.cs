using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Accounts.PasswordChange;

namespace WidgetDepot.Tests.Features.Accounts.PasswordChange;

public class PasswordChangeHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Customer> SeedCustomerAsync(AppDbContext db, string password = "OldP@ss1!")
    {
        var hasher = new PasswordHasher<Customer>();
        var customer = new Customer
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            CreatedAt = DateTime.UtcNow
        };
        customer.PasswordHash = hasher.HashPassword(customer, password);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer;
    }

    [Fact]
    public async Task ChangeAsync_CorrectCurrentPassword_UpdatesHashAndReturnsSuccess()
    {
        using var db = CreateDb();
        var customer = await SeedCustomerAsync(db, "OldP@ss1!");
        var originalHash = customer.PasswordHash;
        var handler = new PasswordChangeHandler(db);
        var request = new ChangePasswordRequest("OldP@ss1!", "NewP@ss2!");

        var result = await handler.ChangeAsync(customer.Id, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ChangePasswordSuccess>();

        var updated = await db.Customers.SingleAsync(TestContext.Current.CancellationToken);
        updated.PasswordHash.ShouldNotBe(originalHash);

        var hasher = new PasswordHasher<Customer>();
        var verify = hasher.VerifyHashedPassword(updated, updated.PasswordHash, "NewP@ss2!");
        verify.ShouldNotBe(PasswordVerificationResult.Failed);
    }

    [Fact]
    public async Task ChangeAsync_IncorrectCurrentPassword_ReturnsIncorrectPassword()
    {
        using var db = CreateDb();
        var customer = await SeedCustomerAsync(db, "OldP@ss1!");
        var handler = new PasswordChangeHandler(db);
        var request = new ChangePasswordRequest("WrongPassword!", "NewP@ss2!");

        var result = await handler.ChangeAsync(customer.Id, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<PasswordChangeError.IncorrectPassword>();
    }

    [Fact]
    public async Task ChangeAsync_UnknownCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = new PasswordChangeHandler(db);
        var request = new ChangePasswordRequest("OldP@ss1!", "NewP@ss2!");

        var result = await handler.ChangeAsync(99, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<PasswordChangeError.NotFound>();
    }
}