using Microsoft.EntityFrameworkCore;
using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Widgets.Import;

public record ImportResult(int Inserted, int Updated, int Skipped);

public class ImportWidgetsCsvHandler(AppDbContext db)
{
    private static readonly string[] ExpectedHeaders = ["SKU", "Name", "Description", "Manufacturer", "Weight", "Price", "Date Available"];

    public async Task<ImportResult?> HandleAsync(Stream csvStream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(csvStream);

        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (headerLine is null)
            return null;

        var headers = headerLine.Split(',');
        foreach (var expected in ExpectedHeaders)
        {
            if (!Array.Exists(headers, h => h.Trim().Equals(expected, StringComparison.OrdinalIgnoreCase)))
                return null;
        }

        var rows = new List<(string Sku, string Name, string Description, string Manufacturer, decimal Weight, decimal Price, DateOnly DateAvailable)>();

        int skipped = 0;
        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = line.Split(',');
            if (cols.Length < 7) { skipped++; continue; }

            var sku = cols[0].Trim();
            var name = cols[1].Trim();
            var description = cols[2].Trim();
            var manufacturer = cols[3].Trim();
            var weightStr = cols[4].Trim();
            var priceStr = cols[5].Trim();
            var dateStr = cols[6].Trim();

            if (string.IsNullOrEmpty(sku) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(priceStr))
            { skipped++; continue; }

            if (!decimal.TryParse(priceStr, out var price))
            { skipped++; continue; }

            if (!DateOnly.TryParse(dateStr, out var dateAvailable))
            { skipped++; continue; }

            decimal.TryParse(weightStr, out var weight);

            rows.Add((sku, name, description, manufacturer, weight, price, dateAvailable));
        }

        var skus = rows.Select(r => r.Sku).ToList();
        var existing = await db.Widgets
            .Where(w => skus.Contains(w.Sku))
            .ToDictionaryAsync(w => w.Sku, cancellationToken);

        var toAdd = new List<Widget>();
        int updated = 0;

        foreach (var row in rows)
        {
            if (existing.TryGetValue(row.Sku, out var widget))
            {
                widget.Name = row.Name;
                widget.Description = row.Description;
                widget.Manufacturer = row.Manufacturer;
                widget.Weight = row.Weight;
                widget.Price = row.Price;
                widget.DateAvailable = row.DateAvailable;
                updated++;
            }
            else
            {
                toAdd.Add(new Widget
                {
                    Sku = row.Sku,
                    Name = row.Name,
                    Description = row.Description,
                    Manufacturer = row.Manufacturer,
                    Weight = row.Weight,
                    Price = row.Price,
                    DateAvailable = row.DateAvailable
                });
            }
        }

        db.Widgets.AddRange(toAdd);
        await db.SaveChangesAsync(cancellationToken);

        return new ImportResult(toAdd.Count, updated, skipped);
    }
}
