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
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

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

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasOne(oi => oi.Widget)
                  .WithMany()
                  .HasForeignKey(oi => oi.WidgetId);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasMany(o => o.Items)
                  .WithOne()
                  .HasForeignKey(oi => oi.OrderId);

            entity.OwnsOne(o => o.ShippingAddress, a =>
            {
                a.Property(x => x.RecipientName).HasColumnName("ShippingRecipientName").HasColumnType("text");
                a.Property(x => x.StreetLine1).HasColumnName("ShippingStreetLine1").HasColumnType("text");
                a.Property(x => x.StreetLine2).HasColumnName("ShippingStreetLine2").HasColumnType("text");
                a.Property(x => x.City).HasColumnName("ShippingCity").HasColumnType("text");
                a.Property(x => x.State).HasColumnName("ShippingState").HasColumnType("text");
                a.Property(x => x.ZipCode).HasColumnName("ShippingZipCode").HasColumnType("text");
            });
            entity.Navigation(o => o.ShippingAddress).IsRequired(false);

            entity.OwnsOne(o => o.BillingAddress, a =>
            {
                a.Property(x => x.RecipientName).HasColumnName("BillingRecipientName").HasColumnType("text");
                a.Property(x => x.StreetLine1).HasColumnName("BillingStreetLine1").HasColumnType("text");
                a.Property(x => x.StreetLine2).HasColumnName("BillingStreetLine2").HasColumnType("text");
                a.Property(x => x.City).HasColumnName("BillingCity").HasColumnType("text");
                a.Property(x => x.State).HasColumnName("BillingState").HasColumnType("text");
                a.Property(x => x.ZipCode).HasColumnName("BillingZipCode").HasColumnType("text");
            });
            entity.Navigation(o => o.BillingAddress).IsRequired(false);

            entity.Property(o => o.ShippingEstimate).HasPrecision(10, 2);
        });

    }
}