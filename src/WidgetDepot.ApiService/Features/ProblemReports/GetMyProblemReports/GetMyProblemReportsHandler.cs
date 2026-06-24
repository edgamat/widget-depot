using Microsoft.EntityFrameworkCore;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Shared;

namespace WidgetDepot.ApiService.Features.ProblemReports.GetMyProblemReports;

public record GetMyProblemReportsResponseItem(
    int Id,
    int OrderId,
    DateTime OrderSubmittedAt,
    DateTime ReportFiledAt,
    bool EmailSent);

public record GetMyProblemReportsQuery(int CustomerId) : IRequest<IReadOnlyList<GetMyProblemReportsResponseItem>>;

public class GetMyProblemReportsHandler : IRequestHandler<GetMyProblemReportsQuery, IReadOnlyList<GetMyProblemReportsResponseItem>>
{
    private readonly AppDbContext _db;

    public GetMyProblemReportsHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<GetMyProblemReportsResponseItem>> HandleAsync(GetMyProblemReportsQuery query, CancellationToken cancellationToken)
    {
        return await _db.ProblemReports
            .Include(pr => pr.Order)
            .Where(pr => pr.Order.CustomerId == query.CustomerId)
            .OrderByDescending(pr => pr.CreatedAt)
            .Take(10)
            .Select(pr => new GetMyProblemReportsResponseItem(
                pr.Id,
                pr.OrderId,
                pr.Order.SubmittedAt ?? DateTime.MinValue,
                pr.CreatedAt,
                pr.EmailSent))
            .ToListAsync(cancellationToken);
    }
}