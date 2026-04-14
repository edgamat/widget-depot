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

    public DbSet<Widget> Widgets => Set<Widget>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Widget>(entity =>
        {
            entity.HasIndex(w => w.Sku).IsUnique();
            entity.Property(w => w.Name).UseCollation("C");
            entity.Property(w => w.Description).UseCollation("C");
            entity.Property(w => w.Weight).HasPrecision(10, 3);
            entity.Property(w => w.Price).HasPrecision(10, 2);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(c => c.Email).IsUnique();
        });
    }
}