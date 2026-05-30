namespace WidgetDepot.AppHost;

public static class FtpContainerExtensions
{
    public const int FtpHostPort = 2121;

    public static IResourceBuilder<ContainerResource> AddFakeFtp(this IDistributedApplicationBuilder builder)
    {
        const int passivePortStart = 21000;
        const int passivePortEnd = 21010;

        var ftp = builder
            .AddContainer("fake-ftp", "delfer/alpine-ftp-server")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithContainerName("fake-ftp")
            .WithEnvironment("USERS", "devftp|devftp|/home/devftp")
            .WithEnvironment("ADDRESS", "127.0.0.1")
            .WithEnvironment("MIN_PORT", passivePortStart.ToString())
            .WithEnvironment("MAX_PORT", passivePortEnd.ToString())
            .WithEndpoint(
                port: FtpHostPort,
                targetPort: 21,
                scheme: "ftp",
                name: "ftp",
                isProxied: false);

        for (var port = passivePortStart; port <= passivePortEnd; port++)
        {
            ftp = ftp.WithEndpoint(
                port: port,
                targetPort: port,
                scheme: "tcp",
                name: $"ftp-passive-{port}",
                isProxied: false);
        }

        return ftp;
    }
}