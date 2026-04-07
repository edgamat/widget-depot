using Microsoft.EntityFrameworkCore;
using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.SearchWidgets;

namespace WidgetDepot.Tests.Features.SearchWidgets;

public class SearchWidgetsHandlerTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
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
        var results = await handler.HandleAsync(new SearchWidgetsQuery(term));

        Assert.Empty(results);
    }
}
