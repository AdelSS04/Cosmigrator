# Cosmigrator

A migration framework for Azure Cosmos DB. Version-controlled, reversible schema changes for your NoSQL documents.

[![CI/CD](https://github.com/AdelSS04/Cosmigrator/workflows/CI%2FCD/badge.svg)](https://github.com/AdelSS04/Cosmigrator/actions)
[![NuGet](https://img.shields.io/nuget/v/Cosmigrator.svg)](https://www.nuget.org/packages/Cosmigrator/)
[![Downloads](https://img.shields.io/nuget/dt/Cosmigrator.svg)](https://www.nuget.org/packages/Cosmigrator/)
[![codecov](https://codecov.io/gh/AdelSS04/Cosmigrator/branch/main/graph/badge.svg)](https://codecov.io/gh/AdelSS04/Cosmigrator)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## What it does

Cosmigrator manages document schema changes in Cosmos DB the same way EF Core migrations handle relational databases — except it's built for NoSQL. Write C# migration classes, run them forward or roll them back from the CLI.

**Supports .NET 8, .NET 9, and .NET 10.**

## Install

```bash
dotnet add package Cosmigrator
```

## Quick start

### 1. Create a console app

```bash
dotnet new console -n MyService.Migrations
cd MyService.Migrations
dotnet add package Cosmigrator
```

### 2. Add `appsettings.json`

```json
{
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://your-account.documents.azure.com:443/;AccountKey=your-key",
    "DatabaseName": "MyDatabase"
  }
}
```

### 3. Write `Program.cs`

One line. `MigrationHost.RunAsync` handles host building, Serilog setup, Cosmos client creation, and CLI parsing.

```csharp
using System.Reflection;
using Cosmigrator;

await MigrationHost.RunAsync(args, Assembly.GetExecutingAssembly());
```

### 4. Add a migration

Create a `Migrations/` folder and add your first migration:

```csharp
using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

public class _20250219_000001_AddEmailToUsers : IMigration
{
    public string Id => "20250219_000001";
    public string Name => "AddEmailToUsers";
    public string ContainerName => "Users";
    public object? DefaultValue => "";

    public async Task UpAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var helper = new BulkOperationHelper(loggerFactory.CreateLogger<BulkOperationHelper>());

        var docs = await helper.ReadAllDocumentsAsync(container);
        var toUpdate = docs.Where(d => d["email"] == null).ToList();

        foreach (var doc in toUpdate)
            doc["email"] = JsonValue.Create(DefaultValue);

        if (toUpdate.Count > 0)
            await helper.BulkUpsertAsync(container, toUpdate);
    }

    public async Task DownAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var helper = new BulkOperationHelper(loggerFactory.CreateLogger<BulkOperationHelper>());

        var docs = await helper.ReadAllDocumentsAsync(container);

        foreach (var doc in docs)
            doc.Remove("email");

        await helper.BulkUpsertAsync(container, docs);
    }
}
```

### 5. Run it

```bash
dotnet run                           # Apply all pending migrations
dotnet run -- rollback               # Undo the last migration
dotnet run -- rollback --steps 3     # Undo the last 3
dotnet run -- status                 # Show applied vs pending
dotnet run -- list                   # Show all discovered migrations
```

## Prerequisites

Cosmigrator does **not** create containers automatically. You must provision them yourself (Terraform, Bicep, Azure Portal, etc.) before running migrations.

| Container | Partition Key | Purpose |
|-----------|---------------|---------|
| `__MigrationHistory` | `/id` | Tracks applied migrations |
| Your target containers | Per your schema | Where migrations operate |

## The `IMigration` interface

Every migration implements this interface:

```csharp
public interface IMigration
{
    string Id { get; }              // "20250219_000001" — used for ordering
    string Name { get; }            // Human-readable description
    string ContainerName { get; }   // Target container
    object? DefaultValue => null;   // Optional default for property additions

    Task UpAsync(Container container, CosmosClient client);
    Task DownAsync(Container container, CosmosClient client);
}
```

Migrations are discovered automatically via reflection and executed in `Id` order.

## Migration scenarios

The [sample project](samples/Cosmigrator.Sample/) includes working examples for common patterns:

| Migration | What it does |
|-----------|-------------|
| `AddAgePropertyToUsers` | Adds a property with a default value to all documents |
| `RemoveMiddleNameProperty` | Removes a property from all documents |
| `RenameUserNameToDisplayName` | Renames a property while preserving data |
| `AddUniqueKeyPolicyToOrders` | Changes unique key policy by recreating the container |
| `AddCompositeIndexToUsers` | Adds a composite index to the indexing policy |

## Advanced: custom host setup

If you need control over configuration and logging, use the overload that accepts `IConfiguration` and `ILoggerFactory`:

```csharp
using System.Reflection;
using Cosmigrator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .AddCommandLine(args);
    })
    .UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration))
    .Build();

var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
var configuration = host.Services.GetRequiredService<IConfiguration>();

await MigrationHost.RunAsync(configuration, loggerFactory, args, Assembly.GetExecutingAssembly());
```

This is useful when you want to:
- Customize Serilog sinks (Seq, Application Insights, etc.)
- Add environment-specific configuration
- Integrate with an existing host pipeline

## Deployment

Cosmigrator exits with code `0` on success and `1` on failure. This makes it easy to use as:

- A **Kubernetes init container** — block the main pod until migrations pass
- A **CI/CD pipeline step** — fail the deployment if a migration fails
- A **Docker Compose dependency** — run before your app starts

```yaml
# Example: Kubernetes init container
initContainers:
  - name: migrations
    image: myregistry/myservice-migrations:latest
    command: ["dotnet", "MyService.Migrations.dll"]
```

## Project structure

```
src/Cosmigrator/              Core library (the NuGet package)
samples/Cosmigrator.Sample/   Example console app with 5 migration scenarios
tests/Cosmigrator.Tests/      Unit tests (xUnit + FluentAssertions)
```

## Contributing

Contributions welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a PR.

```bash
git clone https://github.com/AdelSS04/Cosmigrator.git
cd Cosmigrator
dotnet build
dotnet test
```

## License

[MIT](LICENSE)
