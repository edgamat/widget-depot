using System.Net;

namespace WidgetDepot.Web.Features.ProblemReports.MyProblemReports;

public class MyProblemReportsService
{
    private readonly HttpClient _httpClient;

    public MyProblemReportsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetMyProblemReportsResult> GetMyProblemReportsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/problem-reports", cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var reports = await response.Content.ReadFromJsonAsync<List<GetMyProblemReportsApiResponse>>(cancellationToken);
            if (reports is null)
                return new GetMyProblemReportsResult.Failure();

            var items = reports
                .Select(r => new ProblemReportListItem(r.Id, r.OrderId, r.OrderSubmittedAt, r.ReportFiledAt, r.EmailSent))
                .ToList();

            return new GetMyProblemReportsResult.Success(items);
        }

        return new GetMyProblemReportsResult.Failure();
    }

    public async Task<ResendEmailResult> ResendEmailAsync(int problemReportId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsync($"/problem-reports/{problemReportId}/resend", null, cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
            return new ResendEmailResult.Success();

        return new ResendEmailResult.Failure();
    }
}