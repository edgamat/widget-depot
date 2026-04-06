using Microsoft.EntityFrameworkCore;

namespace WidgetDepot.ApiService.Data;

/// <summary>
/// Application database context for Widget Depot.
/// Contains all entity mappings for the application.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entities here as features are added
    }
}
