---
title: Installation
sidebar_position: 1
---

# Installation

## Package

Install Cosmigrator from NuGet:

```bash
dotnet add package Cosmigrator
```

Or with the .NET CLI in a new console project:

```bash
dotnet new console -n MyService.Migrations
cd MyService.Migrations
dotnet add package Cosmigrator
```

## Requirements

- **.NET 8**, **.NET 9**, or **.NET 10**
- An Azure Cosmos DB account with a provisioned database
- The `__MigrationHistory` container must exist in your database (create it via Terraform, Bicep, or the Azure Portal)

## Container prerequisites

Cosmigrator does **not** create containers automatically. You must provision them before running migrations.

| Container | Partition Key | Purpose |
|-----------|---------------|---------|
| `__MigrationHistory` | `/id` | Tracks which migrations have been applied |
| Your target containers | Per your schema | Where migrations read/write documents |

## Configuration

Add an `appsettings.json` to your migration project:

```json
{
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://your-account.documents.azure.com:443/;AccountKey=your-key",
    "DatabaseName": "MyDatabase"
  }
}
```

The connection string and database name can also be set via environment variables:

```bash
CosmosDb__ConnectionString=AccountEndpoint=https://...
CosmosDb__DatabaseName=MyDatabase
```

## Next steps

Once installed, follow the [Quick Start](./quick-start) guide to write your first migration.
