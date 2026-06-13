namespace WidgetDepot.ApiService.Features.Admin.Orders.GetAdminOrderByNumber;

public static class GetAdminOrderByNumberEndpoint
{
    public static IEndpointRouteBuilder MapGetAdminOrderByNumber(this IEndpointRouteBuilder app)
    {
        app.MapGet("/admin/orders/{orderNumber:int}", async (
            int orderNumber,
            GetAdminOrderByNumberHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(orderNumber, cancellationToken);

            return result switch
            {
                GetAdminOrderByNumberNotFound => Results.NotFound(),
                GetAdminOrderByNumberResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("GetAdminOrderByNumber")
        .RequireAuthorization("IsAdmin");

        return app;
    }
}