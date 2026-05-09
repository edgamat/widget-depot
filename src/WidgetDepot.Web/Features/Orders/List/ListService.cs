using System.Net;
using System.Net.Http.Json;

namespace WidgetDepot.Web.Features.Orders.List;

public class ListService
{
    private readonly HttpClient _httpClient;

    public ListService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetDraftsResult> GetDraftsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/orders/drafts", cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var drafts = await response.Content.ReadFromJsonAsync<List<GetDraftsResponse>>(cancellationToken);
            if (drafts is null)
                return new GetDraftsResult.Failure();

            var items = drafts
                .Select(d => new DraftOrderListItem(d.Id, d.WidgetCount, d.CreatedAt, d.CreatedAt.AddDays(30)))
                .ToList();

            return new GetDraftsResult.Success(items);
        }

        return new GetDraftsResult.Failure();
    }

    public async Task<DeleteDraftResult> DeleteDraftAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"/orders/{orderId}/draft", cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.NoContent => new DeleteDraftResult.Success(),
            HttpStatusCode.NotFound => new DeleteDraftResult.NotFound(),
            HttpStatusCode.Forbidden => new DeleteDraftResult.Forbidden(),
            _ => new DeleteDraftResult.Failure()
        };
    }
}