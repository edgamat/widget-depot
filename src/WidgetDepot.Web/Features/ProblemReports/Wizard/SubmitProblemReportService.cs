using System.Net;

namespace WidgetDepot.Web.Features.ProblemReports.Wizard;

public class SubmitProblemReportService
{
    private readonly HttpClient _httpClient;

    public SubmitProblemReportService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SubmitProblemReportResult> SubmitAsync(
        int orderId,
        IReadOnlyList<IssueSpecificationModel> items,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var body = new SubmitProblemReportBody(
            orderId,
            [.. items.Select(i => new SubmitProblemReportItemBody(i.OrderItemId, i.IssueType!))],
            notes);

        var response = await _httpClient.PostAsJsonAsync("/problem-reports", body, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Created)
            return new SubmitProblemReportResult.Success(0);

        return new SubmitProblemReportResult.Failure();
    }
}