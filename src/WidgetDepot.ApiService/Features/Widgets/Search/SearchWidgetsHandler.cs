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

        var term = query.Term.Trim().ToLower();

        return await db.Widgets
            .Where(w => w.Name.ToLower().Contains(term) ||
                        w.Description.ToLower().Contains(term))
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
