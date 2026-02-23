---
title: Quick Start
sidebar_position: 2
---

# Quick Start

Get from zero to a working migration in 5 minutes.

## 1. Create a console app

```bash
dotnet new console -n MyService.Migrations
cd MyService.Migrations
dotnet add package Cosmigrator
```

## 2. Add configuration

Create `appsettings.json`:

```json
{
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://your-account.documents.azure.com:443/;AccountKey=your-key",
    "DatabaseName": "MyDatabase"
  }
}
```

Make sure it's copied to output:

```xml
<ItemGroup>
  <None Update="appsettings.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

## 3. Write Program.cs

One line. `MigrationHost.RunAsync` handles host building, Serilog setup, Cosmos client creation, and CLI argument parsing.

```csharp
using System.Reflection;
using Cosmigrator;

await MigrationHost.RunAsync(args, Assembly.GetExecutingAssembly());
```

## 4. Create your first migration

Create a `Migrations/` folder and add a migration class:

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
        var helper = new BulkOperationHelper(
            loggerFactory.CreateLogger<BulkOperationHelper>());

        var docs = await helper.ReadDocumentsAsync(
            container,
            "SELECT * FROM c WHERE NOT IS_DEFINED(c.email)");

        foreach (var doc in docs)
            doc["email"] = JsonValue.Create(DefaultValue);

        if (docs.Count > 0)
            await helper.BulkUpsertAsync(container, docs);
    }

    public async Task DownAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var helper = new BulkOperationHelper(
            loggerFactory.CreateLogger<BulkOperationHelper>());

        var docs = await helper.ReadDocumentsAsync(
            container,
            "SELECT * FROM c WHERE IS_DEFINED(c.email)");

        foreach (var doc in docs)
            doc.Remove("email");

        if (docs.Count > 0)
            await helper.BulkUpsertAsync(container, docs);
    }
}
```

## 5. Run it

```bash
# Apply all pending migrations
dotnet run

# Check status
dotnet run -- status

# Roll back the last migration
dotnet run -- rollback
```

## What happens

1. `MigrationHost.RunAsync` builds a .NET Generic Host with Serilog logging
2. Reads `CosmosDb:ConnectionString` and `CosmosDb:DatabaseName` from config
3. Creates a `CosmosClient` with System.Text.Json serialization and bulk execution enabled
4. `MigrationDiscovery.DiscoverAll` scans the assembly for all `IMigration` implementations
5. `MigrationRunner` compares discovered migrations against `__MigrationHistory`
6. Pending migrations execute in `Id` order — each calls `UpAsync`, then gets recorded

## Next steps

- Learn about the [CLI commands](./cli-commands) in detail
- Understand [how it works](../core-concepts/how-it-works) under the hood
- See all [migration scenarios](../migrations/add-property) with real code
