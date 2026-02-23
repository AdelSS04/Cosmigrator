---
slug: /
title: Cosmigrator
sidebar_label: Introduction
sidebar_position: 1
---

# Cosmigrator

A migration framework for Azure Cosmos DB. Version-controlled, reversible schema changes for your NoSQL documents.

Cosmigrator manages document schema changes in Cosmos DB the same way EF Core migrations handle relational databases — except it's built for NoSQL. Write C# migration classes, run them forward or roll them back from the CLI.

## What it does

- **Add, remove, or rename properties** across all documents in a container
- **Modify indexing policies** — add composite indexes, change included paths
- **Swap containers** to change unique key policies (which Cosmos DB doesn't allow in-place)
- **Track history** — every applied migration is recorded with timestamps and status
- **Roll back** — undo the last N migrations when something goes wrong
- **Bulk operations** — batched upserts with automatic 429 retry and exponential backoff

## Supported frameworks

Cosmigrator targets .NET 8, .NET 9, and .NET 10.

## Quick example

```csharp
// Program.cs — one line
using System.Reflection;
using Cosmigrator;

await MigrationHost.RunAsync(args, Assembly.GetExecutingAssembly());
```

```bash
dotnet run                        # Apply all pending migrations
dotnet run -- rollback            # Undo the last migration
dotnet run -- rollback --steps 3  # Undo the last 3
dotnet run -- status              # Show applied vs pending
dotnet run -- list                # List all discovered migrations
```

## Next steps

- [Installation](./getting-started/installation) — install the NuGet package
- [Quick Start](./getting-started/quick-start) — create your first migration in 5 minutes
- [How it works](./core-concepts/how-it-works) — understand the architecture
- [API Reference](./api-reference/imigration) — full method signatures and types
