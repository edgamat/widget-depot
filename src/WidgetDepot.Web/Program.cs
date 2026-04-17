using Microsoft.AspNetCore.Authentication.Cookies;

using WidgetDepot.Web.Components;
using WidgetDepot.Web.Features.Accounts.Register;
using WidgetDepot.Web.Features.Admin.CatalogImport;
using WidgetDepot.Web.Features.Catalog;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/accounts/login";
    });
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHttpClient<CatalogService>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"));

builder.Services.AddHttpClient<CatalogImportService>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"));

builder.Services.AddHttpClient<RegisterService>(client =>
    client.BaseAddress = new Uri("https+http://apiservice"));

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

app.Run();