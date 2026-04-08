using Microsoft.EntityFrameworkCore;
using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Widgets.Search;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.AddNpgsqlDbContext<AppDbContext>("widgetdepot");
builder.Services.AddScoped<SearchWidgetsHandler>();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "API service is running.");

app.MapApiEndpoints();

app.MapDefaultEndpoints();

app.Run();
