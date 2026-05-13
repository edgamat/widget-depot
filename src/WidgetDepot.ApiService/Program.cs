using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Accounts.Login;
using WidgetDepot.ApiService.Features.Accounts.PasswordChange;
using WidgetDepot.ApiService.Features.Accounts.Profile;
using WidgetDepot.ApiService.Features.Accounts.Register;
using WidgetDepot.ApiService.Features.Orders.CalculateShipping;
using WidgetDepot.ApiService.Features.Orders.CreateDraft;
using WidgetDepot.ApiService.Features.Orders.DeleteDraft;
using WidgetDepot.ApiService.Features.Orders.ExpireDraftOrders;
using WidgetDepot.ApiService.Features.Orders.GetDraftOrder;
using WidgetDepot.ApiService.Features.Orders.GetDrafts;
using WidgetDepot.ApiService.Features.Orders.GetRecentSubmitted;
using WidgetDepot.ApiService.Features.Orders.SaveAddresses;
using WidgetDepot.ApiService.Features.Orders.Submit;
using WidgetDepot.ApiService.Features.Orders.UpdateDraftItems;
using WidgetDepot.ApiService.Features.Widgets.Import;
using WidgetDepot.ApiService.Features.Widgets.Search;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddDataProtection()
    .SetApplicationName("WidgetDepot");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();

builder.AddNpgsqlDbContext<AppDbContext>("widgetdepot");

builder.Services.AddHttpClient<IShippingApiClient, AcmeShippingApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://shippingapi/");
    var apiKey = builder.Configuration["SHIPPING_API_KEY"] ?? "dev-api-key";
    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
});
builder.Services.AddScoped<CreateDraftOrderHandler>();
builder.Services.AddScoped<DeleteDraftHandler>();
builder.Services.AddScoped<GetDraftOrderHandler>();
builder.Services.AddScoped<GetDraftsHandler>();
builder.Services.AddScoped<SaveAddressesHandler>();
builder.Services.AddScoped<CalculateShippingHandler>();
builder.Services.AddScoped<SearchWidgetsHandler>();
builder.Services.AddScoped<ImportWidgetsCsvHandler>();
builder.Services.AddScoped<RegisterHandler>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<ProfileHandler>();
builder.Services.AddScoped<PasswordChangeHandler>();
builder.Services.AddScoped<SubmitOrderHandler>();
builder.Services.AddScoped<UpdateDraftItemsHandler>();
builder.Services.AddScoped<GetRecentSubmittedHandler>();
builder.Services.AddScoped<IOrderFileWriter, OrderFileWriter>();
builder.Services.AddScoped<ExpireDraftOrdersHandler>();
builder.Services.AddHostedService<ExpireDraftOrdersJob>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "API service is running.");

app.MapApiEndpoints();

app.MapDefaultEndpoints();

app.Run();