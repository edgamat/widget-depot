using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Admin.Seed;
using WidgetDepot.ApiService.Features.Orders;
using WidgetDepot.ApiService.Features.Orders.CalculateShipping;
using WidgetDepot.ApiService.Features.Orders.ExpireDraftOrders;
using WidgetDepot.ApiService.Features.Orders.Submit;
using WidgetDepot.ApiService.Features.Orders.TransmitOrders;
using WidgetDepot.ApiService.Features.ProblemReports.CreateTestProblemReport;
using WidgetDepot.ApiService.Features.ProblemReports.Email;
using WidgetDepot.ApiService.Shared;

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
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("IsAdmin", policy =>
        policy.RequireClaim("IsAdmin", "true"));
});

builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection("Admin"));
builder.Services.AddScoped<AdminSeeder>();

builder.AddNpgsqlDbContext<AppDbContext>("widgetdepot");

builder.Services.AddHttpClient<IShippingApiClient, AcmeShippingApiClient>(client =>
{
    client.BaseAddress = new Uri("https+http://shippingapi/");
    var apiKey = builder.Configuration["SHIPPING_API_KEY"] ?? "dev-api-key";
    client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
});
builder.Services.AddRequestHandlers();

builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<IProblemReportEmailSender, MailKitProblemReportEmailSender>();

builder.Services.Configure<OrdersOptions>(builder.Configuration.GetSection("Orders"));
builder.Services.AddScoped<IOrderFileWriter, OrderFileWriter>();
builder.Services.AddScoped<IOrderTransmitter, FtpOrderTransmitter>();
builder.Services.AddHostedService<ExpireDraftOrdersJob>();
builder.Services.AddHostedService<TransmitOrdersJob>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Apply migrations and seed at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
    await seeder.SeedAsync(CancellationToken.None);
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapCreateTestProblemReport();
}

app.MapGet("/", () => "API service is running.");

app.MapApiEndpoints();

app.MapDefaultEndpoints();

app.Run();