using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Widgets.Search;

namespace WidgetDepot.Tests.Features.Widgets.Search;

public class SearchWidgetsHandlerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync() => await _postgres.DisposeAsync();

    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;
        var db = new AppDbContext(options);
        db.Widgets.ExecuteDelete();
        return db;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleAsync_NullOrWhitespaceTerm_ReturnsEmpty(string? term)
    {
        using var db = CreateDb();
        db.Widgets.Add(new Widget { Name = "Widget A", Description = "Some description" });
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new SearchWidgetsHandler(db);

        var results = await handler.HandleAsync(new SearchWidgetsQuery(term), TestContext.Current.CancellationToken);

        Assert.Empty(results);
    }

    [Fact]
    public async Task HandleAsync_WithMatchingTerm_ReturnsMatchingWidgets()
    {
        using var db = CreateDb();
        var widget1 = new Widget
        {
            Sku = "SKU001",
            Name = "Premium Gadget",
            Description = "A high-quality device",
            Manufacturer = "TechCorp",
            Weight = 2.5m,
            Price = 99.99m,
            DateAvailable = new DateOnly(2026, 1, 15)
        };
        var widget2 = new Widget
        {
            Sku = "SKU002",
            Name = "Standard Widget",
            Description = "A basic tool for everyday use",
            Manufacturer = "BasicInc",
            Weight = 1.2m,
            Price = 29.99m,
            DateAvailable = new DateOnly(2026, 2, 1)
        };
        var widget3 = new Widget
        {
            Sku = "SKU003",
            Name = "Industrial Tool",
            Description = "Heavy-duty equipment",
            Manufacturer = "Industrial Ltd",
            Weight = 5.0m,
            Price = 199.99m,
            DateAvailable = new DateOnly(2026, 3, 10)
        };

        db.Widgets.AddRange(widget1, widget2, widget3);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var handler = new SearchWidgetsHandler(db);

        var results = await handler.HandleAsync(new SearchWidgetsQuery("Gadget"), TestContext.Current.CancellationToken);

        Assert.Single(results);
        Assert.Equal("SKU001", results[0].Sku);
        Assert.Equal("Premium Gadget", results[0].Name);
        Assert.Equal("A high-quality device", results[0].Description);
        Assert.Equal("TechCorp", results[0].Manufacturer);
        Assert.Equal(2.5m, results[0].Weight);
        Assert.Equal(99.99m, results[0].Price);
        Assert.Equal(new DateOnly(2026, 1, 15), results[0].DateAvailable);
    }
}
