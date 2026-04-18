using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Accounts.Profile;

namespace WidgetDepot.Tests.Features.Accounts.Profile;

public class ProfileHandlerTests
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

    // GET tests

    [Fact]
    public async Task GetAsync_ExistingCustomer_ReturnsProfile()
    {
        using var db = CreateDb();
        SeedCustomer(db);
        var handler = new ProfileHandler(db);

        var result = await handler.GetAsync(1, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<GetProfileResponse>();
        response.FirstName.ShouldBe("Jane");
        response.LastName.ShouldBe("Doe");
        response.Email.ShouldBe("jane@example.com");
    }

    [Fact]
    public async Task GetAsync_UnknownCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = new ProfileHandler(db);

        var result = await handler.GetAsync(99, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ProfileError.NotFound>();
    }

    // UPDATE tests

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesCustomerAndReturnsResponse()
    {
        using var db = CreateDb();
        SeedCustomer(db);
        var handler = new ProfileHandler(db);
        var request = new UpdateProfileRequest("Janet", "Smith", "janet@example.com");

        var result = await handler.UpdateAsync(1, request, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<UpdateProfileResponse>();
        response.FirstName.ShouldBe("Janet");
        response.LastName.ShouldBe("Smith");
        response.Email.ShouldBe("janet@example.com");

        var customer = await db.Customers.SingleAsync(TestContext.Current.CancellationToken);
        customer.FirstName.ShouldBe("Janet");
        customer.LastName.ShouldBe("Smith");
        customer.Email.ShouldBe("janet@example.com");
    }

    [Fact]
    public async Task UpdateAsync_SameEmail_Succeeds()
    {
        using var db = CreateDb();
        SeedCustomer(db);
        var handler = new ProfileHandler(db);
        var request = new UpdateProfileRequest("Jane", "Doe", "jane@example.com");

        var result = await handler.UpdateAsync(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<UpdateProfileResponse>();
    }

    [Fact]
    public async Task UpdateAsync_EmailUsedByAnotherCustomer_ReturnsEmailAlreadyRegistered()
    {
        using var db = CreateDb();
        SeedCustomer(db, id: 1, email: "jane@example.com");
        SeedCustomer(db, id: 2, email: "other@example.com");
        var handler = new ProfileHandler(db);
        var request = new UpdateProfileRequest("Jane", "Doe", "other@example.com");

        var result = await handler.UpdateAsync(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ProfileError.EmailAlreadyRegistered>();
    }

    [Fact]
    public async Task UpdateAsync_UnknownCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = new ProfileHandler(db);
        var request = new UpdateProfileRequest("Jane", "Doe", "jane@example.com");

        var result = await handler.UpdateAsync(99, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ProfileError.NotFound>();
    }
}