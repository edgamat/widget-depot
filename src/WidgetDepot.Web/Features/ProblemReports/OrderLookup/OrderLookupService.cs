using System.Net;

namespace WidgetDepot.Web.Features.ProblemReports.OrderLookup;

public class OrderLookupService
{
    private readonly HttpClient _httpClient;

    public OrderLookupService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LookupOrderResult> LookupOrderAsync(int orderNumber, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/problem-reports/order-lookup/{orderNumber}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return new LookupOrderResult.NotFound();

        if (response.StatusCode == HttpStatusCode.UnprocessableEntity)
            return new LookupOrderResult.NotSubmitted();

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var dto = await response.Content.ReadFromJsonAsync<GetOrderForProblemReportResponse>(cancellationToken);
            if (dto is null)
                return new LookupOrderResult.Failure();

            return new LookupOrderResult.Success(
                dto.OrderId,
                [.. dto.Items.Select(i => new ProblemReportOrderItem(i.WidgetName, i.Quantity))]);
        }

        return new LookupOrderResult.Failure();
    }
}