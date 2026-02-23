---
title: Custom Host Setup
sidebar_position: 3
---

# Custom Host Setup

`MigrationHost.RunAsync` provides a simplified entry point that handles everything. If you need more control, use the overload that accepts `IConfiguration` and `ILoggerFactory`.

## Default setup (one line)

```csharp
using System.Reflection;
using Cosmigrator;

await MigrationHost.RunAsync(args, Assembly.GetExecutingAssembly());
```

This internally:
1. Creates a .NET Generic Host with `Host.CreateDefaultBuilder`
2. Configures Serilog with console output
3. Loads `appsettings.json`, environment variables, and CLI args
4. Creates a `CosmosClient` and runs migrations

## Custom setup

Use the `RunAsync(IConfiguration, ILoggerFactory, string[], Assembly)` overload when you need:
- Custom Serilog sinks (Seq, Application Insights, file)
- Environment-specific configuration
- Integration with an existing host pipeline

```csharp
using System.Reflection;
using Cosmigrator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);
    })
    .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration))
    .Build();

var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
var configuration = host.Services.GetRequiredService<IConfiguration>();

await MigrationHost.RunAsync(
    configuration,
    loggerFactory,
    args,
    Assembly.GetExecutingAssembly());
```

## What `MigrationHost.RunAsync` reads from configuration

| Key | Required | Description |
|-----|----------|-------------|
| `CosmosDb:ConnectionString` | Yes | Full Cosmos DB connection string |
| `CosmosDb:DatabaseName` | Yes | Target database name |

## CosmosClient configuration

Both overloads create a `CosmosClient` with these settings:

```csharp
new CosmosClient(connectionString, new CosmosClientOptions
{
    UseSystemTextJsonSerializerWithOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    },
    AllowBulkExecution = true,
    MaxRetryAttemptsOnRateLimitedRequests = 10,
    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(60)
});
```

Key settings:
- **System.Text.Json** with camelCase naming
- **Bulk execution** enabled for `BulkOperationHelper`
- **10 retry attempts** on rate-limited requests with up to 60s wait
