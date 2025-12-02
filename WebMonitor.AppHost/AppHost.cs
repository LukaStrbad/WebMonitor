using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var backend = builder.AddProject<Projects.WebMonitor>("webmonitor");

var webPort = builder.Configuration.GetValue<int>("Ports:Web", 4200);

builder.AddYarnApp("frontend", "../Frontend", scriptName: "start:aspire")
    .WithHttpEndpoint(env: "PORT", port: webPort)
    .WithExternalHttpEndpoints()
    .WithReference(backend)
    .WaitFor(backend);

builder.Build().Run();
