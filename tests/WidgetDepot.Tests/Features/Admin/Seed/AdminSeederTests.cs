using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Shouldly;

using WidgetDepot.ApiService.Data;
using WidgetDepot.ApiService.Features.Admin.Seed;

namespace WidgetDepot.Tests.Features.Admin.Seed;

public class AdminSeederTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static AdminSeeder CreateSeeder(AppDbContext db, AdminSeedOptions seedOptions)
    {
        var options = Options.Create(seedOptions);
        var logger = NullLogger<AdminSeeder>.Instance;
        return new AdminSeeder(db, options, logger);
    }

    [Fact]
    public async Task SeedAsync_ValidCredentials_CreatesAdminUser()
    {
        using var db = CreateDb();
        var seeder = CreateSeeder(db, new AdminSeedOptions
        {
            SeedCredentials = new SeedCredentials { UserName = "admin", Password = "P@ssw0rd" }
        });
        var ct = TestContext.Current.CancellationToken;

        await seeder.SeedAsync(ct);

        var admin = await db.Customers.SingleOrDefaultAsync(c => c.Email == "admin", ct);
        admin.ShouldNotBeNull();
        admin.IsAdmin.ShouldBeTrue();
    }

    [Fact]
    public async Task SeedAsync_RunTwice_DoesNotCreateDuplicate()
    {
        using var db = CreateDb();
        var seeder = CreateSeeder(db, new AdminSeedOptions
        {
            SeedCredentials = new SeedCredentials { UserName = "admin", Password = "P@ssw0rd" }
        });
        var ct = TestContext.Current.CancellationToken;

        await seeder.SeedAsync(ct);
        await seeder.SeedAsync(ct);

        var count = await db.Customers.CountAsync(c => c.Email == "admin", ct);
        count.ShouldBe(1);
    }

    [Fact]
    public async Task SeedAsync_SeedCredentialsAbsent_SkipsSeeding()
    {
        using var db = CreateDb();
        var seeder = CreateSeeder(db, new AdminSeedOptions { SeedCredentials = null });
        var ct = TestContext.Current.CancellationToken;

        await seeder.SeedAsync(ct);

        var count = await db.Customers.CountAsync(ct);
        count.ShouldBe(0);
    }

    [Fact]
    public async Task SeedAsync_EmptyUserName_SkipsSeeding()
    {
        using var db = CreateDb();
        var seeder = CreateSeeder(db, new AdminSeedOptions
        {
            SeedCredentials = new SeedCredentials { UserName = "", Password = "P@ssw0rd" }
        });
        var ct = TestContext.Current.CancellationToken;

        await seeder.SeedAsync(ct);

        var count = await db.Customers.CountAsync(ct);
        count.ShouldBe(0);
    }
}