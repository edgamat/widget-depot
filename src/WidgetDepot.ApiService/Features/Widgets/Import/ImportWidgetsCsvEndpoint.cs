namespace WidgetDepot.ApiService.Features.Widgets.Import;

public static class ImportWidgetsCsvEndpoint
{
    public static IEndpointRouteBuilder MapImportWidgetsCsv(this IEndpointRouteBuilder app)
    {
        app.MapPost(WidgetEndpoints.Import, async (IFormFile file, ImportWidgetsCsvHandler handler, CancellationToken cancellationToken) =>
        {
            await using var stream = file.OpenReadStream();
            var result = await handler.HandleAsync(stream, cancellationToken);

            if (result is null)
                return Results.Problem("The CSV file is missing expected column headers.", statusCode: 400);

            return Results.Ok(result);
        })
        .DisableAntiforgery()
        .WithName("ImportWidgetsCsv");

        return app;
    }
}