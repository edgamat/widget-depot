using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Accounts.Register;

namespace WidgetDepot.Tests.Features.Accounts.Register;

public class RegisterHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_NewEmail_CreatesCustomerAndReturnsResponse()
    {
        using var db = CreateDb();
        var handler = new RegisterHandler(db);
        var request = new RegisterRequest("Jane", "Doe", "jane@example.com", "P@ssw0rd!");

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<RegisterResponse>();
        response.CustomerId.ShouldBeGreaterThan(0);

        var customer = await db.Customers.SingleAsync(TestContext.Current.CancellationToken);
        customer.Email.ShouldBe("jane@example.com");
        customer.FirstName.ShouldBe("Jane");
        customer.LastName.ShouldBe("Doe");
        customer.PasswordHash.ShouldNotBe("P@ssw0rd!");
        customer.PasswordHash.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_DuplicateEmail_ReturnsEmailAlreadyRegisteredError()
    {
        using var db = CreateDb();
        var handler = new RegisterHandler(db);
        var request = new RegisterRequest("Jane", "Doe", "jane@example.com", "P@ssw0rd!");

        await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        var result = await handler.HandleAsync(request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<RegisterError.EmailAlreadyRegistered>();
        (await db.Customers.CountAsync(TestContext.Current.CancellationToken)).ShouldBe(1);
    }
}