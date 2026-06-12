using System.Security.Claims;

namespace WidgetDepot.ApiService.Features.Admin.Customers.DemoteCustomer;

public static class DemoteCustomerEndpoint
{
    public static IEndpointRouteBuilder MapDemoteCustomer(this IEndpointRouteBuilder app)
    {
        app.MapPut("/admin/customers/{id:int}/demote", async (int id, ClaimsPrincipal user, DemoteCustomerHandler handler, CancellationToken cancellationToken) =>
        {
            if (!user.TryGetCustomerId(out var requestingAdminId))
                return Results.Unauthorized();

            var result = await handler.DemoteAsync(id, requestingAdminId, cancellationToken);

            return result switch
            {
                DemoteCustomerError.NotFound => Results.NotFound(),
                DemoteCustomerError.CannotDemoteSelf => Results.Problem(
                    detail: "An admin cannot remove their own admin rights.",
                    statusCode: 409,
                    extensions: new Dictionary<string, object?> { ["errorCode"] = "CannotDemoteSelf" }),
                DemoteCustomerSuccess => Results.Ok(),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("DemoteAdminCustomer")
        .RequireAuthorization("IsAdmin");

        return app;
    }
}