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
}