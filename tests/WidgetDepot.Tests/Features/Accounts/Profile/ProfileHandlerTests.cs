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
    public async Task GetAsync_CustomerWithNoAddresses_ReturnsNullAddresses()
    {
        using var db = CreateDb();
        SeedCustomer(db);
        var handler = new ProfileHandler(db);

        var result = await handler.GetAsync(1, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<GetProfileResponse>();
        response.ShippingAddress.ShouldBeNull();
        response.BillingAddress.ShouldBeNull();
    }

    [Fact]
    public async Task GetAsync_CustomerWithAddresses_ReturnsAddresses()
    {
        using var db = CreateDb();
        var customer = SeedCustomer(db);
        customer.ShippingAddress = new Address { RecipientName = "Jane Doe", StreetLine1 = "123 Main St", City = "Springfield", State = "IL", ZipCode = "62701" };
        customer.BillingAddress = new Address { RecipientName = "Jane Doe", StreetLine1 = "456 Oak Ave", City = "Shelbyville", State = "IL", ZipCode = "62565" };
        db.SaveChanges();
        var handler = new ProfileHandler(db);

        var result = await handler.GetAsync(1, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<GetProfileResponse>();
        response.ShippingAddress.ShouldNotBeNull();
        response.ShippingAddress.RecipientName.ShouldBe("Jane Doe");
        response.ShippingAddress.StreetLine1.ShouldBe("123 Main St");
        response.ShippingAddress.City.ShouldBe("Springfield");
        response.ShippingAddress.State.ShouldBe("IL");
        response.ShippingAddress.ZipCode.ShouldBe("62701");
        response.BillingAddress.ShouldNotBeNull();
        response.BillingAddress.StreetLine1.ShouldBe("456 Oak Ave");
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
        var request = new UpdateProfileRequest("Janet", "Smith", "janet@example.com", null, null);

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
    public async Task UpdateAsync_WithAddresses_PersistsAddresses()
    {
        using var db = CreateDb();
        SeedCustomer(db);
        var handler = new ProfileHandler(db);
        var shippingAddress = new AddressDto("Jane Doe", "123 Main St", null, "Springfield", "IL", "62701");
        var billingAddress = new AddressDto("Jane Doe", "456 Oak Ave", "Apt 2", "Shelbyville", "IL", "62565");
        var request = new UpdateProfileRequest("Jane", "Doe", "jane@example.com", shippingAddress, billingAddress);

        var result = await handler.UpdateAsync(1, request, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<UpdateProfileResponse>();
        response.ShippingAddress.ShouldNotBeNull();
        response.ShippingAddress.RecipientName.ShouldBe("Jane Doe");
        response.ShippingAddress.StreetLine1.ShouldBe("123 Main St");
        response.ShippingAddress.City.ShouldBe("Springfield");
        response.BillingAddress.ShouldNotBeNull();
        response.BillingAddress.StreetLine1.ShouldBe("456 Oak Ave");
        response.BillingAddress.StreetLine2.ShouldBe("Apt 2");

        var customer = await db.Customers.SingleAsync(TestContext.Current.CancellationToken);
        customer.ShippingAddress.ShouldNotBeNull();
        customer.ShippingAddress.StreetLine1.ShouldBe("123 Main St");
        customer.BillingAddress.ShouldNotBeNull();
        customer.BillingAddress.StreetLine1.ShouldBe("456 Oak Ave");
    }

    [Fact]
    public async Task UpdateAsync_WithNullAddresses_ClearsAddresses()
    {
        using var db = CreateDb();
        var customer = SeedCustomer(db);
        customer.ShippingAddress = new Address { RecipientName = "Jane Doe", StreetLine1 = "123 Main St", City = "Springfield", State = "IL", ZipCode = "62701" };
        db.SaveChanges();
        var handler = new ProfileHandler(db);
        var request = new UpdateProfileRequest("Jane", "Doe", "jane@example.com", null, null);

        var result = await handler.UpdateAsync(1, request, TestContext.Current.CancellationToken);

        var response = result.ShouldBeOfType<UpdateProfileResponse>();
        response.ShippingAddress.ShouldBeNull();

        var saved = await db.Customers.SingleAsync(TestContext.Current.CancellationToken);
        saved.ShippingAddress.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateAsync_SameEmail_Succeeds()
    {
        using var db = CreateDb();
        SeedCustomer(db);
        var handler = new ProfileHandler(db);
        var request = new UpdateProfileRequest("Jane", "Doe", "jane@example.com", null, null);

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
        var request = new UpdateProfileRequest("Jane", "Doe", "other@example.com", null, null);

        var result = await handler.UpdateAsync(1, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ProfileError.EmailAlreadyRegistered>();
    }

    [Fact]
    public async Task UpdateAsync_UnknownCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = new ProfileHandler(db);
        var request = new UpdateProfileRequest("Jane", "Doe", "jane@example.com", null, null);

        var result = await handler.UpdateAsync(99, request, TestContext.Current.CancellationToken);

        result.ShouldBeOfType<ProfileError.NotFound>();
    }
}