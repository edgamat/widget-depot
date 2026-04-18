namespace WidgetDepot.ApiService.Features.Accounts.Profile;

public static class ProfileEndpoint
{
    public static IEndpointRouteBuilder MapProfile(this IEndpointRouteBuilder app)
    {
        app.MapGet("/accounts/profile/{customerId:int}", async (int customerId, ProfileHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.GetAsync(customerId, cancellationToken);

            return result switch
            {
                ProfileError.NotFound => Results.NotFound(),
                GetProfileResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("GetProfile");

        app.MapPut("/accounts/profile/{customerId:int}", async (int customerId, UpdateProfileRequest request, ProfileHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.UpdateAsync(customerId, request, cancellationToken);

            return result switch
            {
                ProfileError.NotFound => Results.NotFound(),
                ProfileError.EmailAlreadyRegistered => Results.Problem(
                    detail: "The email address is already registered.",
                    statusCode: 409,
                    extensions: new Dictionary<string, object?> { ["errorCode"] = "EmailAlreadyRegistered" }),
                UpdateProfileResponse response => Results.Ok(response),
                _ => Results.Problem(statusCode: 500)
            };
        })
        .WithName("UpdateProfile");

        return app;
    }
}