using WidgetDepot.ApiService.Features.Accounts.Register;

namespace WidgetDepot.ApiService.Features.Accounts;

public static class AccountEndpointExtensions
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapRegister();

        return app;
    }
}