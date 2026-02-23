---
title: MigrationHost
sidebar_position: 2
---

# MigrationHost

Static entry point for running Cosmos DB migrations. Handles infrastructure bootstrapping: configuration loading, Cosmos client creation, CLI parsing, and delegation to `MigrationRunner`.

```csharp
namespace Cosmigrator;

public static class MigrationHost
{
    public static async Task RunAsync(string[] args, Assembly migrationAssembly);

    public static async Task RunAsync(
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        string[] args,
        Assembly migrationAssembly);
}
```

## Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `RunAsync(string[], Assembly)` | `Task` | Simplified entry point — builds a host with Serilog, loads config, creates CosmosClient, and runs migrations |
| `RunAsync(IConfiguration, ILoggerFactory, string[], Assembly)` | `Task` | Advanced entry point — uses provided configuration and logging instead of building its own host |

## RunAsync (simple)

```csharp
public static async Task RunAsync(string[] args, Assembly migrationAssembly)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `args` | `string[]` | Command line arguments. First argument is the command (`migrate`, `rollback`, `status`, `list`) |
| `migrationAssembly` | `Assembly` | The assembly containing `IMigration` implementations |

### What it does

1. Creates a Serilog logger with console output
2. Builds a `Host` with `Host.CreateDefaultBuilder`
3. Configures `appsettings.json`, environment variables, and CLI args
4. Creates a `CosmosClient` with System.Text.Json and bulk execution
5. Instantiates `MigrationRunner` and parses the CLI command
6. On fatal error, exits with code `1`

### Usage

```csharp
using System.Reflection;
using Cosmigrator;

await MigrationHost.RunAsync(args, Assembly.GetExecutingAssembly());
```

## RunAsync (advanced)

```csharp
public static async Task RunAsync(
    IConfiguration configuration,
    ILoggerFactory loggerFactory,
    string[] args,
    Assembly migrationAssembly)
```

### Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `configuration` | `IConfiguration` | Must contain `CosmosDb:ConnectionString` and `CosmosDb:DatabaseName` |
| `loggerFactory` | `ILoggerFactory` | Logger factory for creating typed loggers |
| `args` | `string[]` | CLI arguments for command parsing |
| `migrationAssembly` | `Assembly` | Assembly to scan for migrations |

### Required configuration keys

| Key | Description |
|-----|-------------|
| `CosmosDb:ConnectionString` | Full Cosmos DB connection string |
| `CosmosDb:DatabaseName` | Target database name |

Throws `Exception` if either key is missing.

### Usage

```csharp
using System.Reflection;
using Cosmigrator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config.AddJsonFile("appsettings.json").AddEnvironmentVariables();
    })
    .UseSerilog()
    .Build();

await MigrationHost.RunAsync(
    host.Services.GetRequiredService<IConfiguration>(),
    host.Services.GetRequiredService<ILoggerFactory>(),
    args,
    Assembly.GetExecutingAssembly());
```

## CLI commands

Both overloads parse the first CLI argument:

| Command | Description |
|---------|-------------|
| `migrate` (default) | Apply all pending migrations |
| `rollback` | Roll back the last migration. Use `--steps N` for multiple |
| `status` | Print migration status |
| `list` | List all discovered migrations |
