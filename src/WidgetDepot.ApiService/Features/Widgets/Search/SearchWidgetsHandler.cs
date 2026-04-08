using Microsoft.EntityFrameworkCore;
using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Widgets.Search;

public record SearchWidgetsQuery(string? Term);

public record WidgetResult(
    int Id,
    string Sku,
    string Name,
    string Description,
    string Manufacturer,
    decimal Weight,
    decimal Price,
    DateOnly DateAvailable);

public class SearchWidgetsHandler(AppDbContext db)
{
    public async Task<IReadOnlyList<WidgetResult>> HandleAsync(SearchWidgetsQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Term))
            return [];

        var term = query.Term.Trim();

        return await db.Widgets
            .Where(w => EF.Functions.ILike(w.Name, $"%{term}%") ||
                        EF.Functions.ILike(w.Description, $"%{term}%"))
            .Select(w => new WidgetResult(
                w.Id,
                w.Sku,
                w.Name,
                w.Description,
                w.Manufacturer,
                w.Weight,
                w.Price,
                w.DateAvailable))
            .ToListAsync(cancellationToken);
    }
}
