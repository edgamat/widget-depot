using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddDataProtection()
    .SetApplicationName("WidgetDepot");


var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/", () => "API service is running.");

app.MapDefaultEndpoints();

var acceptedApiKey = builder.Configuration["ShippingApiKey"] ?? "dev-api-key";

app.MapPost("/estimates", (HttpRequest request, EstimateRequest? body) =>
{
    if (!request.Headers.TryGetValue("X-Api-Key", out var apiKey) || apiKey != acceptedApiKey)
        return Results.Unauthorized();

    if (body is null
        || body.Origin is null
        || string.IsNullOrEmpty(body.Origin.PostalCode)
        || string.IsNullOrEmpty(body.Origin.Country)
        || body.Destination is null
        || string.IsNullOrEmpty(body.Destination.PostalCode)
        || string.IsNullOrEmpty(body.Destination.Country)
        || body.Package is null
        || body.Package.WeightLbs is null)
    {
        return Results.BadRequest();
    }

    var rate = Random.Shared.NextDouble() * 0.1 + 0.1;
    var estimatedCost = Math.Round(rate * body.Package.WeightLbs.Value, 2);

    return Results.Ok(new { estimatedCost, currency = "USD" });
});

app.Run();

record Address(string? PostalCode, string? Country);
record PackageInfo(double? WeightLbs);
record EstimateRequest(Address? Origin, Address? Destination, PackageInfo? Package);