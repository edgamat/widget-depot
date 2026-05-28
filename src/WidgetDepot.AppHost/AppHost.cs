using System.Net.Sockets;

var builder = DistributedApplication.CreateBuilder(args);

const int ftpHostPort = 2121;
const int passivePortStart = 21000;
const int passivePortEnd = 21010;

var ftp = builder
    .AddContainer("fake-ftp", "delfer/alpine-ftp-server")
    .WithEnvironment("USERS", "devftp|devftp|/home/devftp")
    .WithEnvironment("ADDRESS", "127.0.0.1")
    .WithEnvironment("MIN_PORT", passivePortStart.ToString())
    .WithEnvironment("MAX_PORT", passivePortEnd.ToString())
    .WithEndpoint(
        port: ftpHostPort,
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

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithContainerName("postgres")
    .WithDataVolume()
    .WithEndpoint(
        name: "postgres",
        port: 5432,
        targetPort: 5432,
        protocol: ProtocolType.Tcp,
        isProxied: false
    )
    .AddDatabase("widgetdepot");

var fakeShippingApi = builder.AddProject<Projects.WidgetDepot_FakeShippingApi>("shippingapi")
    .WithHttpHealthCheck("/health");

var apiService = builder.AddProject<Projects.WidgetDepot_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(postgres)
    .WaitFor(postgres)
    .WithReference(fakeShippingApi)
    .WaitFor(fakeShippingApi)
    .WithEnvironment("FtpTransmission__Host", "127.0.0.1")
    .WithEnvironment("FtpTransmission__Port", ftpHostPort.ToString())
    .WithEnvironment("FtpTransmission__Username", "devftp")
    .WithEnvironment("FtpTransmission__Password", "devftp")
    .WithEnvironment("FtpTransmission__RemoteDirectory", "/home/devftp");

builder.AddProject<Projects.WidgetDepot_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();