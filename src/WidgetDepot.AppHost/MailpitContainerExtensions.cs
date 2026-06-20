namespace WidgetDepot.AppHost;

public static class MailpitContainerExtensions
{
    public const int SmtpHostPort = 1025;
    public const int WebUiHostPort = 8025;

    public static IResourceBuilder<ContainerResource> AddMailpit(this IDistributedApplicationBuilder builder)
    {
        return builder
            .AddContainer("mailpit", "axllent/mailpit")
            .WithLifetime(ContainerLifetime.Persistent)
            .WithContainerName("mailpit")
            .WithEndpoint(
                port: SmtpHostPort,
                targetPort: 1025,
                scheme: "tcp",
                name: "smtp",
                isProxied: false)
            .WithEndpoint(
                port: WebUiHostPort,
                targetPort: 8025,
                scheme: "http",
                name: "http",
                isProxied: false);
    }
}