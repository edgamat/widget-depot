using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using WidgetDepot.ApiService.Data;

namespace WidgetDepot.ApiService.Features.Admin.Seed;

public class AdminSeeder(AppDbContext db, IOptions<AdminSeedOptions> options, ILogger<AdminSeeder> logger)
{
    private readonly PasswordHasher<Customer> _passwordHasher = new();

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var credentials = options.Value.SeedCredentials;

        if (credentials is null || string.IsNullOrWhiteSpace(credentials.UserName) || string.IsNullOrWhiteSpace(credentials.Password))
        {
            logger.LogWarning("Admin seed credentials are not configured. Skipping admin user seeding.");
            return;
        }

        var exists = await db.Customers
            .AnyAsync(c => c.Email == credentials.UserName, cancellationToken);

        if (exists)
            return;

        var admin = new Customer
        {
            FirstName = "Admin",
            LastName = "User",
            Email = credentials.UserName,
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow
        };

        admin.PasswordHash = _passwordHasher.HashPassword(admin, credentials.Password);

        db.Customers.Add(admin);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Admin user '{UserName}' created.", credentials.UserName);
    }
}