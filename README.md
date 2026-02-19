<div align="center">

# Cosmigrator

**A powerful, flexible migration framework for Azure Cosmos DB**

[![Build Status](https://github.com/AdelSS04/Cosmigrator/workflows/CI%2FCD/badge.svg)](https://github.com/AdelSS04/Cosmigrator/actions)
[![NuGet](https://img.shields.io/nuget/v/Cosmigrator.svg)](https://www.nuget.org/packages/Cosmigrator/)
[![codecov](https://codecov.io/gh/AdelSS04/Cosmigrator/branch/main/graph/badge.svg)](https://codecov.io/gh/AdelSS04/Cosmigrator)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/download)

[Features](#-features) • [Quick Start](#-quick-start) • [Documentation](#-documentation) • [Contributing](#-contributing) • [License](#-license)

</div>

---

## 📋 Overview

Cosmigrator is a .NET 8 migration framework for Azure Cosmos DB, inspired by Entity Framework migrations but tailored for Cosmos DB's document-based, schema-less nature. Manage document schema changes, index updates, and container modifications with confidence through version-controlled, reversible migrations.

### Why Cosmigrator?

- 🚀 **Production-Ready**: Built-in retry logic, bulk operations, and throttling handling
- 🔄 **Reversible**: Every migration supports both `Up` and `Down` operations
- 📦 **Reusable Core**: Single library shared across multiple migration projects
- 🎯 **Type-Safe**: Strong typing with C# instead of JSON scripts
- 🔍 **Discoverable**: Automatic migration discovery via reflection
- 📊 **Tracked**: Built-in migration history tracking
- 🌐 **Serverless-Compatible**: Works with both provisioned and serverless Cosmos DB
- 🔧 **Infrastructure-as-Code**: Designed for Terraform/Bicep workflows

## ✨ Features

- **Document Schema Evolution**: Add, remove, or rename properties across all documents
- **Index Management**: Update indexing policies, add composite indexes
- **Unique Key Policies**: Safely modify unique key constraints via container recreation
- **Bulk Operations**: Efficient bulk document updates with automatic batching
- **Retry Logic**: Built-in exponential backoff for transient failures
- **CLI Interface**: Run, rollback, and check migration status from command line
- **Flexible Logging**: Provider-agnostic logging (works with Serilog, NLog, etc.)
- **System.Text.Json**: Modern JSON serialization without Newtonsoft.Json

## 🚀 Quick Start

### Installation

```bash
dotnet add package Cosmigrator

## Architecture

```
Cosmigrator.slnx
├── src/
│   └── Cosmigrator/                ← Core library (NuGet package)
│       ├── IMigration.cs               ← Migration contract
│       ├── MigrationHost.cs            ← Static entry point (handles all infrastructure)
│       ├── MigrationRunner.cs          ← Orchestrator: run, rollback, status
│       ├── MigrationHistory.cs         ← __MigrationHistory container manager
│       ├── MigrationDiscovery.cs       ← Reflection-based migration scanner
│       ├── BulkOperationHelper.cs      ← Bulk upsert with retry logic
│       └── Models/
│           └── MigrationRecord.cs      ← History record model
│
├── samples/
│   └── Cosmigrator.Sample/            ← Example migration console app
│       ├── Program.cs                  ← Entry point → calls MigrationHost.RunAsync()
│       ├── appsettings.json            ← Cosmos DB + Serilog config
│       └── Migrations/
│           ├── 20240101_000001_AddAgePropertyToUsers.cs
│           ├── 20240101_000002_RemoveMiddleNameProperty.cs
│           ├── 20240101_000003_RenameUserNameToDisplayName.cs
│           ├── 20240101_000004_AddUniqueKeyPolicyToOrders.cs
│           └── 20240101_000005_AddCompositeIndexToUsers.cs
│
├── tests/
│   └── Cosmigrator.Tests/             ← Unit tests (xUnit + FluentAssertions)
│
└── YourService.DbInit/                ← (Your own project, same pattern)
    ├── Program.cs
    ├── appsettings.json
    └── Migrations/
        └── ...
```

### Design Principles

- **Core library** uses only `Microsoft.Extensions.Logging.Abstractions` (`ILogger<T>`, `ILoggerFactory`) — no dependency on Serilog or any specific logging provider.
- **`MigrationHost.RunAsync()`** is the single entry point — handles Cosmos client creation, CLI parsing, and command execution. Each migration project just calls this with its config and logger factory.
- **Each migration project** configures its own Serilog pipeline in `Program.cs` and passes `ILoggerFactory` to `MigrationHost`.
- **No container auto-creation** — all containers (including `__MigrationHistory`) must be provisioned externally (e.g. via Terraform).

---

## Creating a New Migration Project

### 1. Create a new console app

```bash
dotnet new console -n MyService.DbInit
cd MyService.DbInit
dotnet add package Cosmigrator
```

### 2. Add NuGet packages

```bash
dotnet add package Microsoft.Extensions.Hosting
dotnet add package Serilog
dotnet add package Serilog.Extensions.Hosting
dotnet add package Serilog.Settings.Configuration
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Enrichers.Environment
dotnet add package Serilog.Expressions
```

### 3. Create `appsettings.json`

```json
{
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=...;AccountKey=...",
    "DatabaseName": "MyDatabase"
  },
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Enrichers.Environment",
      "Serilog.Expressions"
    ],
    "MinimumLevel": {
      "Default": "Warning"
    },
    "Enrich": [ "FromLogContext", "WithEnvironmentName" ],
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": "Serilog.Formatting.Json.JsonFormatter, Serilog"
        }
      }
    ],
    "Properties": {
      "Application": "myservice-db-init"
    }
  }
}
```

### 4. Create `Program.cs`

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
    .UseSerilog((context, configuration) =>
    {
        configuration.ReadFrom.Configuration(context.Configuration);
    })
    .Build();

var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
var configuration = host.Services.GetRequiredService<IConfiguration>();

try
{
    await MigrationHost.RunAsync(
        configuration,
        loggerFactory,
        args,
        Assembly.GetExecutingAssembly());
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
    Environment.Exit(1);
}
finally
{
    await Log.CloseAndFlushAsync();
}
```

### 5. Create migration files in `Migrations/`

```csharp
using Cosmigrator;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;

public class _20250219_000001_AddEmailToUsers : IMigration
{
    public string Id => "20250219_000001";
    public string Name => "AddEmailToUsers";
    public string ContainerName => "Users";
    public object? DefaultValue => ""; // Optional: default value for property additions

    public async Task UpAsync(Container container, CosmosClient client)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
        var logger = loggerFactory.CreateLogger<_20250219_000001_AddEmailToUsers>();
        var helper = new BulkOperationHelper(
            loggerFactory.CreateLogger<BulkOperationHelper>());

        var docs = await helper.ReadAllDocumentsAsync(container);
        var modified = docs.Where(d => d["email"] == null).ToList();

        foreach (var doc in modified)
            doc["email"] = JsonValue.Create(DefaultValue);

        if (modified.Count > 0)
            await helper.BulkUpsertAsync(container, modified);
    }

    public async Task DownAsync(Container container, CosmosClient client)
    {
        // reverse logic
    }
}
```

---

## CLI Commands

```bash
dotnet run                           # Run all pending migrations (default)
dotnet run -- migrate                # Same as default
dotnet run -- rollback               # Rollback the last applied migration
dotnet run -- rollback --steps 3     # Rollback last 3 migrations
dotnet run -- status                 # Show migration status
dotnet run -- list                   # List all discovered migrations
```

---

## Terraform Prerequisites

All containers must be provisioned externally. Required containers:

| Container | Partition Key | Purpose |
|-----------|---------------|---------|
| `__MigrationHistory` | `/id` | Tracks applied/rolled-back migrations |
| Any container referenced by your migrations | Per your schema | Target of migration operations |

---

## Deployment as Auto-Migration

Run with **no arguments** to auto-apply pending migrations. Ideal for:

- **Kubernetes init containers**
- **Docker Compose `depends_on`**
- **CI/CD pipeline steps**

### Exit Codes

| Code | Meaning |
|------|---------|
| `0` | All migrations applied (or none pending) |
| `1` | A migration failed — pipeline should halt |

---

## 📚 Documentation

### Architecture

```
Cosmigrator.slnx
├── src/Cosmigrator/                # Core library (NuGet package)
├── samples/Cosmigrator.Sample/     # Example project
├── tests/Cosmigrator.Tests/        # Unit tests
└── .github/                        # CI/CD workflows
```

### Design Principles

1. **Provider-Agnostic Logging**: Core library uses `ILogger<T>` only
2. **Single Entry Point**: `MigrationHost.RunAsync()` handles everything
3. **No Auto-Creation**: All containers provisioned via IaC (Terraform/Bicep)
4. **Maximum Reusability**: One Core library, many migration projects

### Migration Scenarios

The sample project includes examples for:

- **Scenario A**: Add property with default value
- **Scenario B**: Remove property
- **Scenario C**: Rename property with data preservation
- **Scenario D**: Update unique key policy (container recreation)
- **Scenario E**: Add composite index

### Best Practices

- ✅ **Version Control**: Commit migrations to source control
- ✅ **Test Rollbacks**: Always test `Down()` in non-production first
- ✅ **Backup Data**: Backup critical containers before running migrations
- ✅ **Idempotent Migrations**: Design migrations to be safely re-runnable
- ✅ **Small Batches**: Prefer small, focused migrations over large ones
- ✅ **Monitor Performance**: Watch RU consumption during bulk operations

### Troubleshooting

**Q: "Container not found" error**  
A: Ensure your Terraform/Bicep has created all required containers before running migrations.

**Q: 429 Throttling errors**  
A: Built-in retry logic handles this automatically. Consider increasing container RUs if persistent.

**Q: Migration history not persisting**  
A: Verify `__MigrationHistory` container exists with `/id` partition key.

**Q: Can't rollback a migration**  
A: Ensure the migration's `DownAsync()` method is properly implemented and tested.

---

## 🤝 Contributing

We love contributions! Here's how you can help:

1. 🐛 **Report bugs** via [GitHub Issues](https://github.com/AdelSS04/Cosmigrator/issues)
2. 💡 **Suggest features** or improvements
3. 📖 **Improve documentation**
4. 🧪 **Write tests** to increase coverage
5. 🔧 **Submit pull requests**

Please read our [Contributing Guidelines](CONTRIBUTING.md) and [Code of Conduct](CODE_OF_CONDUCT.md) before submitting.

### Development Setup

```bash
# Clone the repository
git clone https://github.com/AdelSS04/Cosmigrator.git
cd Cosmigrator

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run sample migrations (requires Cosmos DB)
cd samples/Cosmigrator.Sample
dotnet run -- migrate
```

### Running Tests

```bash
# All tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"

# Specific test class
dotnet test --filter "FullyQualifiedName~MigrationDiscoveryTests"
```

---

## 📊 Project Status

- ✅ Core migration framework complete
- ✅ Bulk operation support with retry logic
- ✅ System.Text.Json integration
- ✅ Comprehensive sample migrations
- ✅ Unit test coverage
- 🚧 Integration tests (in progress)
- 🚧 NuGet package publishing (planned)
- 🚧 Performance benchmarks (planned)

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- Inspired by [Entity Framework Migrations](https://docs.microsoft.com/ef/core/managing-schemas/migrations/)
- Built with [Azure Cosmos DB SDK](https://github.com/Azure/azure-cosmos-dotnet-v3)
- Uses [Serilog](https://serilog.net/) in sample implementations

---

## 📞 Support

- 📖 [Documentation](https://github.com/AdelSS04/Cosmigrator/wiki)
- 💬 [Discussions](https://github.com/AdelSS04/Cosmigrator/discussions)
- 🐛 [Issue Tracker](https://github.com/AdelSS04/Cosmigrator/issues)
- 🔒 [Security Policy](SECURITY.md)

---

## 🌟 Star History

If you find Cosmigrator useful, please consider giving it a star ⭐️

---

<div align="center">

**Made with ❤️ for the Azure Cosmos DB community**

[⬆ Back to Top](#cosmigrator)

</div>
