using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Orders.ExpireDraftOrders;

public class ExpireDraftOrdersHandler
{
    private readonly AppDbContext _db;

    public ExpireDraftOrdersHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> HandleAsync(CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);

        var expired = await _db.Orders
            .Where(o => o.Status == OrderStatus.Draft && o.CreatedAt < cutoff)
            .ToListAsync(cancellationToken);

        _db.Orders.RemoveRange(expired);
        await _db.SaveChangesAsync(cancellationToken);

        return expired.Count;
    }
}