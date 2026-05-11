using System.Net.Sockets;

var builder = DistributedApplication.CreateBuilder(args);

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
    .WaitFor(fakeShippingApi);

builder.AddProject<Projects.WidgetDepot_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();