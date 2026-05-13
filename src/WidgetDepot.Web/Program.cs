using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

using WidgetDepot.Web.Components;
using WidgetDepot.Web.Features.Accounts.Login;
using WidgetDepot.Web.Features.Accounts.PasswordChange;
using WidgetDepot.Web.Features.Accounts.Profile;
using WidgetDepot.Web.Features.Accounts.Register;
using WidgetDepot.Web.Features.Admin.CatalogImport;
using WidgetDepot.Web.Features.Catalog;
using WidgetDepot.Web.Features.Orders.Create;
using WidgetDepot.Web.Features.Orders.Create.Step1;
using WidgetDepot.Web.Features.Orders.Create.Step2;
using WidgetDepot.Web.Features.Orders.Create.Step3;
using WidgetDepot.Web.Features.Orders.Detail;
using WidgetDepot.Web.Features.Orders.History;
using WidgetDepot.Web.Features.Orders.List;
using WidgetDepot.Web.Features.Orders.Submit;
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
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CookieForwardingHandler>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<CatalogService>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"));

builder.Services.AddHttpClient<CatalogImportService>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"));

builder.Services.AddHttpClient<RegisterService>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"));

builder.Services.AddHttpClient<LoginService>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"));

builder.Services.AddHttpClient<ProfileService>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"))
    .AddHttpMessageHandler<CookieForwardingHandler>();

builder.Services.AddHttpClient<PasswordChangeService>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"))
    .AddHttpMessageHandler<CookieForwardingHandler>();

builder.Services.AddHttpClient<Step1Service>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"))
    .AddHttpMessageHandler<CookieForwardingHandler>();

builder.Services.AddHttpClient<Step2Service>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"))
    .AddHttpMessageHandler<CookieForwardingHandler>();

builder.Services.AddHttpClient<Step3Service>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"))
    .AddHttpMessageHandler<CookieForwardingHandler>();

builder.Services.AddHttpClient<ListService>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"))
    .AddHttpMessageHandler<CookieForwardingHandler>();

builder.Services.AddHttpClient<Step4Service>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"))
    .AddHttpMessageHandler<CookieForwardingHandler>();

builder.Services.AddHttpClient<HistoryService>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"))
    .AddHttpMessageHandler<CookieForwardingHandler>();

builder.Services.AddHttpClient<DetailService>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"))
    .AddHttpMessageHandler<CookieForwardingHandler>();

builder.Services.AddScoped<OrderWizardState>();

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

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.MapGet("/accounts/do-signin", async (HttpContext context, int customerId, string email, string firstName, string? returnUrl) =>
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, customerId.ToString()),
        new(ClaimTypes.Email, email),
        new(ClaimTypes.Name, firstName)
    };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
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