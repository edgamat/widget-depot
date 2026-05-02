var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
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