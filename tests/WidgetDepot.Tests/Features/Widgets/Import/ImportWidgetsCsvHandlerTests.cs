using System.Text;
using Microsoft.EntityFrameworkCore;
using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Widgets.Import;

namespace WidgetDepot.Tests.Features.Widgets.Import;

public class ImportWidgetsCsvHandlerTests
{
    private static readonly string ValidHeader = "SKU,Name,Description,Manufacturer,Weight,Price,Date Available";

    private AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Stream ToCsvStream(string csv)
        => new MemoryStream(Encoding.UTF8.GetBytes(csv));

    [Fact]
    public async Task HandleAsync_ValidCsv_InsertsWidgetsAndReturnsInsertedCount()
    {
        using var db = CreateDb();
        var handler = new ImportWidgetsCsvHandler(db);

        var csv = $"""
            {ValidHeader}
            SKU001,Widget A,Desc A,MfrA,1.0,9.99,2026-01-01
            SKU002,Widget B,Desc B,MfrB,2.0,19.99,2026-02-01
            """;

        var result = await handler.HandleAsync(ToCsvStream(csv), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(2, result.Inserted);
        Assert.Equal(0, result.Updated);
        Assert.Equal(0, result.Skipped);
        Assert.Equal(2, await db.Widgets.CountAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task HandleAsync_SameCsvUploadedTwice_UpdatesExistingWidgetsAndReturnsZeroInserted()
    {
        using var db = CreateDb();
        var handler = new ImportWidgetsCsvHandler(db);

        var csv = $"""
            {ValidHeader}
            SKU001,Widget A,Desc A,MfrA,1.0,9.99,2026-01-01
            """;

        await handler.HandleAsync(ToCsvStream(csv), TestContext.Current.CancellationToken);

        var updatedCsv = $"""
            {ValidHeader}
            SKU001,Widget A Updated,Desc A Updated,MfrA,1.5,14.99,2026-06-01
            """;

        var result = await handler.HandleAsync(ToCsvStream(updatedCsv), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(0, result.Inserted);
        Assert.Equal(1, result.Updated);
        Assert.Equal(0, result.Skipped);

        var widget = await db.Widgets.SingleAsync(TestContext.Current.CancellationToken);
        Assert.Equal("Widget A Updated", widget.Name);
        Assert.Equal(14.99m, widget.Price);
    }

    [Fact]
    public async Task HandleAsync_RowWithBlankSku_IsSkipped()
    {
        using var db = CreateDb();
        var handler = new ImportWidgetsCsvHandler(db);

        var csv = $"""
            {ValidHeader}
            ,Widget A,Desc A,MfrA,1.0,9.99,2026-01-01
            SKU002,Widget B,Desc B,MfrB,2.0,19.99,2026-02-01
            """;

        var result = await handler.HandleAsync(ToCsvStream(csv), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(1, result.Inserted);
        Assert.Equal(1, result.Skipped);
    }

    [Fact]
    public async Task HandleAsync_RowWithBlankName_IsSkipped()
    {
        using var db = CreateDb();
        var handler = new ImportWidgetsCsvHandler(db);

        var csv = $"""
            {ValidHeader}
            SKU001,,Desc A,MfrA,1.0,9.99,2026-01-01
            SKU002,Widget B,Desc B,MfrB,2.0,19.99,2026-02-01
            """;

        var result = await handler.HandleAsync(ToCsvStream(csv), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(1, result.Inserted);
        Assert.Equal(1, result.Skipped);
    }

    [Fact]
    public async Task HandleAsync_RowWithBlankPrice_IsSkipped()
    {
        using var db = CreateDb();
        var handler = new ImportWidgetsCsvHandler(db);

        var csv = $"""
            {ValidHeader}
            SKU001,Widget A,Desc A,MfrA,1.0,,2026-01-01
            SKU002,Widget B,Desc B,MfrB,2.0,19.99,2026-02-01
            """;

        var result = await handler.HandleAsync(ToCsvStream(csv), TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(1, result.Inserted);
        Assert.Equal(1, result.Skipped);
    }

    [Fact]
    public async Task HandleAsync_MissingRequiredColumnHeader_ReturnsNull()
    {
        using var db = CreateDb();
        var handler = new ImportWidgetsCsvHandler(db);

        var csv = """
            SKU,Name,Description,Manufacturer,Weight,Price
            SKU001,Widget A,Desc A,MfrA,1.0,9.99
            """;

        var result = await handler.HandleAsync(ToCsvStream(csv), TestContext.Current.CancellationToken);

        Assert.Null(result);
        Assert.Equal(0, await db.Widgets.CountAsync(TestContext.Current.CancellationToken));
    }
}
