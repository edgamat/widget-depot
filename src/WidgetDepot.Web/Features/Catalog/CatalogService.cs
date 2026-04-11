namespace WidgetDepot.Web.Features.Catalog;

public class CatalogService(HttpClient httpClient)
{
    public async Task<IReadOnlyList<WidgetResult>> SearchAsync(string? term)
    {
        var results = await httpClient.GetFromJsonAsync<List<WidgetResult>>(
            $"/widgets/search?term={Uri.EscapeDataString(term ?? string.Empty)}");

        return results ?? [];
    }
}