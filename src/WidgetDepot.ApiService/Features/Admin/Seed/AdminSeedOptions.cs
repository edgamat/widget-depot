namespace WidgetDepot.ApiService.Features.Admin.Seed;

public class AdminSeedOptions
{
    public SeedCredentials? SeedCredentials { get; set; }
}

public class SeedCredentials
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}