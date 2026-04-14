namespace WidgetDepot.ApiService.Features.Accounts.Register;

public static class RegisterEndpoint
{
    public static IEndpointRouteBuilder MapRegister(this IEndpointRouteBuilder app)
    {
        app.MapPost("/accounts/register", async (RegisterRequest request, RegisterHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);

            return result switch
            {
                RegisterError.EmailAlreadyRegistered => Results.Problem(
                    detail: "The email address is already registered.",
                    statusCode: 409,
                    extensions: new Dictionary<string, object?> { ["errorCode"] = "EmailAlreadyRegistered" }),
                RegisterResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("Register");

        return app;
    }
}