using System.Net;

namespace WidgetDepot.Web.Features.Orders.Create.Step1;

public class Step1Service(HttpClient httpClient)
{
    public async Task<GetDraftStep1Result> GetDraftOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"/orders/{orderId}/draft", cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var order = await response.Content.ReadFromJsonAsync<GetDraftStep1Response>(cancellationToken);
            return order is null
                ? new GetDraftStep1Result.Failure()
                : new GetDraftStep1Result.Success(order);
        }

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => new GetDraftStep1Result.NotFound(),
            HttpStatusCode.Forbidden => new GetDraftStep1Result.Forbidden(),
            HttpStatusCode.Conflict => new GetDraftStep1Result.NotDraft(),
            _ => new GetDraftStep1Result.Failure()
        };
    }

    public async Task<IReadOnlyList<WidgetSearchResult>> SearchWidgetsAsync(string term, CancellationToken cancellationToken = default)
    {
        var results = await httpClient.GetFromJsonAsync<List<WidgetSearchResult>>(
            $"/widgets/search?term={Uri.EscapeDataString(term)}",
            cancellationToken);

        return results ?? [];
    }

    public async Task<CreateDraftResult> CreateDraftAsync(IReadOnlyList<OrderItemModel> items, CancellationToken cancellationToken = default)
    {
        var request = new CreateDraftRequest(
            items.Select(i => new CreateDraftItemRequest(i.WidgetId, i.Quantity)).ToList());

        var response = await httpClient.PostAsJsonAsync("/orders/draft", request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var draft = await response.Content.ReadFromJsonAsync<CreateDraftResponse>(cancellationToken);
            return new CreateDraftResult.Success(draft!.OrderId);
        }

        return new CreateDraftResult.Failure();
    }

    public async Task<UpdateDraftResult> UpdateDraftAsync(int orderId, IReadOnlyList<OrderItemModel> items, CancellationToken cancellationToken = default)
    {
        var request = new UpdateDraftRequest(
            items.Select(i => new UpdateDraftItemRequest(i.WidgetId, i.Quantity)).ToList());

        var response = await httpClient.PutAsJsonAsync($"/orders/{orderId}/items", request, cancellationToken);

        return response.StatusCode == System.Net.HttpStatusCode.NoContent
            ? new UpdateDraftResult.Success()
            : new UpdateDraftResult.Failure();
    }
}