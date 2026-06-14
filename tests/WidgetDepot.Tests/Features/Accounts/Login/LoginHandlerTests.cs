using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Accounts.Login;

namespace WidgetDepot.Tests.Features.Accounts.Login;

public class LoginHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<Customer> SeedCustomerAsync(AppDbContext db, string email = "jane@example.com", string password = "P@ssw0rd!")
    {
        var hasher = new PasswordHasher<Customer>();
        var customer = new Customer
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
        customer.PasswordHash = hasher.HashPassword(customer, password);
        db.Customers.Add(customer);
        await db.SaveChangesAsync();
        return customer;
    }

    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsLoginResponse()
    {
        using var db = CreateDb();
        await SeedCustomerAsync(db);
        var handler = new LoginHandler(db);
        var request = new LoginRequest("jane@example.com", "P@ssw0rd!");

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<LoginResponse>();
        response.Email.ShouldBe("jane@example.com");
        response.FirstName.ShouldBe("Jane");
        response.CustomerId.ShouldBeGreaterThan(0);
        response.IsAdmin.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_ValidCredentials_ReturnsMustChangePasswordFalseByDefault()
    {
        using var db = CreateDb();
        await SeedCustomerAsync(db);
        var handler = new LoginHandler(db);
        var request = new LoginRequest("jane@example.com", "P@ssw0rd!");

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<LoginResponse>();
        response.MustChangePassword.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleAsync_CustomerWithMustChangePassword_ReturnsMustChangePasswordTrue()
    {
        using var db = CreateDb();
        var hasher = new PasswordHasher<Customer>();
        var customer = new Customer
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com",
            MustChangePassword = true,
            CreatedAt = DateTime.UtcNow
        };
        customer.PasswordHash = hasher.HashPassword(customer, "P@ssw0rd!");
        db.Customers.Add(customer);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new LoginHandler(db);

        var result = await handler.HandleAsync(new LoginRequest("jane@example.com", "P@ssw0rd!"), TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<LoginResponse>();
        response.MustChangePassword.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAsync_AdminUser_ReturnsIsAdminTrue()
    {
        using var db = CreateDb();
        var hasher = new PasswordHasher<Customer>();
        var admin = new Customer
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = hasher.HashPassword(admin, "P@ssw0rd!");
        db.Customers.Add(admin);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        var handler = new LoginHandler(db);

        var result = await handler.HandleAsync(new LoginRequest("admin@example.com", "P@ssw0rd!"), TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<LoginResponse>();
        response.IsAdmin.ShouldBeTrue();
    }

    [Fact]
    public async Task HandleAsync_UnknownEmail_ReturnsInvalidCredentials()
    {
        using var db = CreateDb();
        var handler = new LoginHandler(db);
        var request = new LoginRequest("unknown@example.com", "P@ssw0rd!");

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<LoginError.InvalidCredentials>();
    }

    [Fact]
    public async Task HandleAsync_WrongPassword_ReturnsInvalidCredentials()
    {
        using var db = CreateDb();
        await SeedCustomerAsync(db);
        var handler = new LoginHandler(db);
        var request = new LoginRequest("jane@example.com", "WrongPassword!");

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<LoginError.InvalidCredentials>();
    }

    [Fact]
    public async Task HandleAsync_DatabaseThrows_ReturnsFailure()
    {
        var db = CreateDb();
        db.Dispose();
        var handler = new LoginHandler(db);
        var request = new LoginRequest("jane@example.com", "P@ssw0rd!");

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<LoginError.Failure>();
    }
}