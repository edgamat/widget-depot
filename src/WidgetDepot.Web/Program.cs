using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

using WidgetDepot.Web.Components;
using WidgetDepot.Web.Features.Admin.Customers;
using WidgetDepot.Web.Features.Orders.Create;
using WidgetDepot.Web.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddDataProtection()
    .SetApplicationName("WidgetDepot");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/accounts/login";
        options.AccessDeniedPath = "/access-denied";
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsAdmin", policy =>
        policy.RequireClaim("IsAdmin", "true"));
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CookieForwardingHandler>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<PaginationOptions>(builder.Configuration.GetSection("Pagination"));
builder.Services.AddApiHttpClients();

builder.Services.AddScoped<OrderWizardState>();

if (builder.Environment.IsDevelopment())
{
    builder.Services
        .AddHttpClient("test-api", c => c.BaseAddress = new Uri("https+http://apiservice"))
        .AddHttpMessageHandler<CookieForwardingHandler>();
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ForcePasswordChangeMiddleware>();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapPost("/test/problem-reports", async (
        TestProblemReportBody body,
        IHttpClientFactory httpClientFactory,
        CancellationToken cancellationToken) =>
    {
        var client = httpClientFactory.CreateClient("test-api");
        var response = await client.PostAsJsonAsync("/test/problem-reports", body, cancellationToken);
        return response.IsSuccessStatusCode
            ? Results.Ok(await response.Content.ReadFromJsonAsync<object>(cancellationToken))
            : Results.Problem();
    }).RequireAuthorization();
}

app.MapGet("/accounts/do-signin", async (HttpContext context, int customerId, string email, string firstName, bool isAdmin, bool mustChangePassword, string? returnUrl) =>
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, customerId.ToString()),
        new(ClaimTypes.Email, email),
        new(ClaimTypes.Name, firstName)
    };

    if (isAdmin)
        claims.Add(new Claim("IsAdmin", "true"));

    if (mustChangePassword)
        claims.Add(new Claim("MustChangePassword", "true"));

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    if (mustChangePassword)
        return Results.Redirect("/accounts/force-password-change");

    var target = !string.IsNullOrEmpty(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative)
        ? returnUrl
        : "/";
    return Results.Redirect(target);
});

app.MapGet("/accounts/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.Run();

record TestProblemReportBody(int OrderId);