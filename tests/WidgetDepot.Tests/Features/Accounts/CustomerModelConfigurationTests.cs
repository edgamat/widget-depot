using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.Tests.Features.Accounts;

public class CustomerModelConfigurationTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void Customer_EmailProperty_HasUniqueIndex()
    {
        using var db = CreateDb();

        var entityType = db.Model.FindEntityType(typeof(Customer));
        var uniqueEmailIndex = entityType!
            .GetIndexes()
            .FirstOrDefault(i => i.IsUnique && i.Properties.Any(p => p.Name == nameof(Customer.Email)));

        uniqueEmailIndex.ShouldNotBeNull();
    }

    [Fact]
    public void Customer_RequiredStringProperties_HaveNonNullableDefaults()
    {
        var customer = new Customer();

        customer.FirstName.ShouldBe(string.Empty);
        customer.LastName.ShouldBe(string.Empty);
        customer.Email.ShouldBe(string.Empty);
        customer.PasswordHash.ShouldBe(string.Empty);
    }
}
