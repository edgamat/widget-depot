namespace WidgetDepot.ApiService.Features.Accounts.Login;

public static class LoginEndpoint
{
    public static IEndpointRouteBuilder MapLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost("/accounts/login", async (LoginRequest request, LoginHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);

            return result switch
            {
                LoginError.InvalidCredentials => Results.Problem(
                    detail: "Invalid email or password.",
                    statusCode: 401,
                    extensions: new Dictionary<string, object?> { ["errorCode"] = "InvalidCredentials" }),
                LoginResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("Login");

        return app;
    }
}