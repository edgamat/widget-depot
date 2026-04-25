namespace WidgetDepot.Web.Features.Orders.Create.Step1;

public class Step1Service(HttpClient httpClient)
{
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
}