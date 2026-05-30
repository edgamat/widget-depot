using System.Net.Sockets;
using WidgetDepot.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var ftp = builder.AddFakeFtp();

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
    .WaitFor(ftp)
    .WithEnvironment("FtpTransmission__Host", "127.0.0.1")
    .WithEnvironment("FtpTransmission__Port", FtpContainerExtensions.FtpHostPort.ToString())
    .WithEnvironment("FtpTransmission__Username", "devftp")
    .WithEnvironment("FtpTransmission__Password", "devftp")
    .WithEnvironment("FtpTransmission__RemoteDirectory", "/home/devftp");

builder.AddProject<Projects.WidgetDepot_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();