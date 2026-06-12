using Microsoft.EntityFrameworkCore;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Admin.Customers.GetCustomerList;

namespace WidgetDepot.Tests.Features.Admin.Customers;

public class GetCustomerListHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static void SeedCustomers(AppDbContext db, int count)
    {
        for (var i = 1; i <= count; i++)
        {
            db.Customers.Add(new Customer
            {
                Id = i,
                FirstName = $"First{i:D2}",
                LastName = $"Last{i:D2}",
                Email = $"customer{i}@example.com",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow
            });
        }
        db.SaveChanges();
    }

    [Fact]
    public async Task GetAsync_NoCustomers_ReturnsEmptyList()
    {
        using var db = CreateDb();
        var handler = new GetCustomerListHandler(db);

        var result = await handler.GetAsync(1, 20, TestContext.Current.CancellationToken);

        result.Customers.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
        result.Page.ShouldBe(1);
        result.PageSize.ShouldBe(20);
    }

    [Fact]
    public async Task GetAsync_FewerThanPageSize_ReturnsAllOnFirstPage()
    {
        using var db = CreateDb();
        SeedCustomers(db, 5);
        var handler = new GetCustomerListHandler(db);

        var result = await handler.GetAsync(1, 20, TestContext.Current.CancellationToken);

        result.Customers.Count.ShouldBe(5);
        result.TotalCount.ShouldBe(5);
    }

    [Fact]
    public async Task GetAsync_ExactlyOneFullPage_ReturnsCorrectCount()
    {
        using var db = CreateDb();
        SeedCustomers(db, 20);
        var handler = new GetCustomerListHandler(db);

        var result = await handler.GetAsync(1, 20, TestContext.Current.CancellationToken);

        result.Customers.Count.ShouldBe(20);
        result.TotalCount.ShouldBe(20);
    }

    [Fact]
    public async Task GetAsync_MoreThanOnePage_FirstPageReturnsPageSizeRows()
    {
        using var db = CreateDb();
        SeedCustomers(db, 25);
        var handler = new GetCustomerListHandler(db);

        var result = await handler.GetAsync(1, 20, TestContext.Current.CancellationToken);

        result.Customers.Count.ShouldBe(20);
        result.TotalCount.ShouldBe(25);
    }

    [Fact]
    public async Task GetAsync_MoreThanOnePage_SecondPageReturnsRemainder()
    {
        using var db = CreateDb();
        SeedCustomers(db, 25);
        var handler = new GetCustomerListHandler(db);

        var result = await handler.GetAsync(2, 20, TestContext.Current.CancellationToken);

        result.Customers.Count.ShouldBe(5);
        result.TotalCount.ShouldBe(25);
        result.Page.ShouldBe(2);
    }

    [Fact]
    public async Task GetAsync_PageBeyondLastPage_ReturnsEmptyList()
    {
        using var db = CreateDb();
        SeedCustomers(db, 20);
        var handler = new GetCustomerListHandler(db);

        var result = await handler.GetAsync(2, 20, TestContext.Current.CancellationToken);

        result.Customers.ShouldBeEmpty();
        result.TotalCount.ShouldBe(20);
    }

    [Fact]
    public async Task GetAsync_CustomersAreSortedByLastNameThenFirstName()
    {
        using var db = CreateDb();
        db.Customers.AddRange(
            new Customer { Id = 1, FirstName = "Bob", LastName = "Zebra", Email = "b@example.com", PasswordHash = "h", CreatedAt = DateTime.UtcNow },
            new Customer { Id = 2, FirstName = "Alice", LastName = "Apple", Email = "a@example.com", PasswordHash = "h", CreatedAt = DateTime.UtcNow },
            new Customer { Id = 3, FirstName = "Charlie", LastName = "Apple", Email = "c@example.com", PasswordHash = "h", CreatedAt = DateTime.UtcNow }
        );
        db.SaveChanges();
        var handler = new GetCustomerListHandler(db);

        var result = await handler.GetAsync(1, 20, TestContext.Current.CancellationToken);

        result.Customers[0].LastName.ShouldBe("Apple");
        result.Customers[0].FirstName.ShouldBe("Alice");
        result.Customers[1].LastName.ShouldBe("Apple");
        result.Customers[1].FirstName.ShouldBe("Charlie");
        result.Customers[2].LastName.ShouldBe("Zebra");
    }

    [Fact]
    public async Task GetAsync_ReturnsIsAdminFlag()
    {
        using var db = CreateDb();
        db.Customers.Add(new Customer
        {
            Id = 1,
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@example.com",
            PasswordHash = "h",
            CreatedAt = DateTime.UtcNow,
            IsAdmin = true
        });
        db.SaveChanges();
        var handler = new GetCustomerListHandler(db);

        var result = await handler.GetAsync(1, 20, TestContext.Current.CancellationToken);

        result.Customers[0].IsAdmin.ShouldBeTrue();
    }
}