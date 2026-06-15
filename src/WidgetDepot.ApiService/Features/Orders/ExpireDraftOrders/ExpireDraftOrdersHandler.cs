using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.Orders.ExpireDraftOrders;

public record ExpireDraftOrdersCommand : IRequest<int>;

public class ExpireDraftOrdersHandler : IRequestHandler<ExpireDraftOrdersCommand, int>
{
    private readonly AppDbContext _db;

    public ExpireDraftOrdersHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> HandleAsync(ExpireDraftOrdersCommand command, CancellationToken cancellationToken)
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