using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.ProblemReports.GetOrderForProblemReport;

public record GetOrderForProblemReportItemResponse(int OrderItemId, string WidgetName, int Quantity);

public record GetOrderForProblemReportResponse(int OrderId, IReadOnlyList<GetOrderForProblemReportItemResponse> Items);

public record GetOrderForProblemReportNotFound;

public record GetOrderForProblemReportNotSubmitted;

public record GetOrderForProblemReportQuery(int OrderNumber, int CustomerId) : IRequest<object>;

public class GetOrderForProblemReportHandler : IRequestHandler<GetOrderForProblemReportQuery, object>
{
    private readonly AppDbContext _db;

    public GetOrderForProblemReportHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<object> HandleAsync(GetOrderForProblemReportQuery query, CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Widget)
            .FirstOrDefaultAsync(o => o.Id == query.OrderNumber && o.CustomerId == query.CustomerId, cancellationToken);

        if (order is null)
            return new GetOrderForProblemReportNotFound();

        if (order.Status != OrderStatus.Submitted)
            return new GetOrderForProblemReportNotSubmitted();

        return new GetOrderForProblemReportResponse(
            order.Id,
            [.. order.Items.Select(i => new GetOrderForProblemReportItemResponse(i.Id, i.Widget.Name, i.Quantity))]);
    }
}