namespace WidgetDepot.Web.Features.Admin.CatalogImport;

public class CatalogImportService(HttpClient httpClient)
{
    public async Task<ImportResult?> ImportAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
        content.Add(fileContent, "file", fileName);

        var response = await httpClient.PostAsync("/widgets/import", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<ImportResult>(cancellationToken);
    }
}