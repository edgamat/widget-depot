namespace WidgetDepot.ApiService.Features.Widgets.Search;

public static class SearchWidgetEndpoint
{
    public static IEndpointRouteBuilder MapSearchWidget(this IEndpointRouteBuilder app)
    {
        app.MapGet(WidgetEndpoints.Search, async (string? term, SearchWidgetsHandler handler, CancellationToken cancellationToken) =>
        {
            var results = await handler.HandleAsync(new SearchWidgetsQuery(term), cancellationToken);

            return Results.Ok(results);
        })
        .WithName("SearchWidgets");

        return app;
    }
}