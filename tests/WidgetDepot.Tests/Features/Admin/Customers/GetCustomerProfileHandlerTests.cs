using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Admin.Customers.GetCustomerProfile;

namespace WidgetDepot.Tests.Features.Admin.Customers;

public class GetCustomerProfileHandlerTests
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
            CreatedAt = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            IsAdmin = isAdmin
        };
        db.Customers.Add(customer);
        db.SaveChanges();
        return customer;
    }

    [Fact]
    public async Task GetAsync_ExistingCustomer_ReturnsProfile()
    {
        using var db = CreateDb();
        SeedCustomer(db);
        var handler = new GetCustomerProfileHandler(db);

        var result = await handler.HandleAsync(new GetCustomerProfileQuery(1), TestContext.Current.CancellationToken);

        var profile = result.ShouldBeOfType<CustomerProfileResponse>();
        profile.Id.ShouldBe(1);
        profile.FirstName.ShouldBe("Jane");
        profile.LastName.ShouldBe("Doe");
        profile.Email.ShouldBe("jane@example.com");
        profile.IsAdmin.ShouldBeFalse();
    }

    [Fact]
    public async Task GetAsync_AdminCustomer_ReturnsIsAdminTrue()
    {
        using var db = CreateDb();
        SeedCustomer(db, isAdmin: true);
        var handler = new GetCustomerProfileHandler(db);

        var result = await handler.HandleAsync(new GetCustomerProfileQuery(1), TestContext.Current.CancellationToken);

        var profile = result.ShouldBeOfType<CustomerProfileResponse>();
        profile.IsAdmin.ShouldBeTrue();
    }

    [Fact]
    public async Task GetAsync_CustomerWithNoAddresses_ReturnsNullAddresses()
    {
        using var db = CreateDb();
        SeedCustomer(db);
        var handler = new GetCustomerProfileHandler(db);

        var result = await handler.HandleAsync(new GetCustomerProfileQuery(1), TestContext.Current.CancellationToken);

        var profile = result.ShouldBeOfType<CustomerProfileResponse>();
        profile.ShippingAddress.ShouldBeNull();
        profile.BillingAddress.ShouldBeNull();
    }

    [Fact]
    public async Task GetAsync_CustomerWithAddresses_ReturnsAddresses()
    {
        using var db = CreateDb();
        var customer = SeedCustomer(db);
        customer.ShippingAddress = new Address { RecipientName = "Jane Doe", StreetLine1 = "123 Main St", City = "Springfield", State = "IL", ZipCode = "62701" };
        customer.BillingAddress = new Address { RecipientName = "Jane Doe", StreetLine1 = "456 Oak Ave", City = "Shelbyville", State = "IL", ZipCode = "62565" };
        db.SaveChanges();
        var handler = new GetCustomerProfileHandler(db);

        var result = await handler.HandleAsync(new GetCustomerProfileQuery(1), TestContext.Current.CancellationToken);

        var profile = result.ShouldBeOfType<CustomerProfileResponse>();
        profile.ShippingAddress.ShouldNotBeNull();
        profile.ShippingAddress.RecipientName.ShouldBe("Jane Doe");
        profile.ShippingAddress.StreetLine1.ShouldBe("123 Main St");
        profile.ShippingAddress.City.ShouldBe("Springfield");
        profile.BillingAddress.ShouldNotBeNull();
        profile.BillingAddress.StreetLine1.ShouldBe("456 Oak Ave");
    }

    [Fact]
    public async Task GetAsync_UnknownCustomer_ReturnsNotFound()
    {
        using var db = CreateDb();
        var handler = new GetCustomerProfileHandler(db);

        var result = await handler.HandleAsync(new GetCustomerProfileQuery(99), TestContext.Current.CancellationToken);

        result.ShouldBeOfType<CustomerProfileError.NotFound>();
    }

    [Fact]
    public async Task GetAsync_ExistingCustomer_ReturnsCreatedAt()
    {
        using var db = CreateDb();
        SeedCustomer(db);
        var handler = new GetCustomerProfileHandler(db);

        var result = await handler.HandleAsync(new GetCustomerProfileQuery(1), TestContext.Current.CancellationToken);

        var profile = result.ShouldBeOfType<CustomerProfileResponse>();
        profile.CreatedAt.ShouldBe(new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc));
    }
}