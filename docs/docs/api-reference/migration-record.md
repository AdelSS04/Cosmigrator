---
title: MigrationRecord
sidebar_position: 7
---

# MigrationRecord

Represents a migration record stored in the `__MigrationHistory` Cosmos DB container. Part of the `Cosmigrator.Models` namespace.

```csharp
namespace Cosmigrator.Models;

public class MigrationRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("appliedAt")]
    public DateTime AppliedAt { get; set; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MigrationStatus Status { get; set; }
}

public enum MigrationStatus
{
    Applied,
    RolledBack
}
```

## MigrationRecord properties

| Property | Type | JSON Key | Description |
|----------|------|----------|-------------|
| `Id` | `string` | `id` | Migration identifier. Also the document ID and partition key |
| `Name` | `string` | `name` | Human-readable name of the migration |
| `AppliedAt` | `DateTime` | `appliedAt` | UTC timestamp of when the migration was applied or rolled back |
| `Status` | `MigrationStatus` | `status` | Current status, serialized as a string (`"Applied"` or `"RolledBack"`) |

## MigrationStatus enum

| Value | Description |
|-------|-------------|
| `Applied` | Migration has been successfully applied via `UpAsync` |
| `RolledBack` | Migration has been rolled back via `DownAsync` |

## JSON representation

A `MigrationRecord` is stored in Cosmos DB as:

```json
{
  "id": "20250219_000001",
  "name": "AddEmailToUsers",
  "appliedAt": "2025-02-19T14:30:00.0000000",
  "status": "Applied"
}
```

The `status` field uses `JsonStringEnumConverter` so it's stored as a human-readable string rather than an integer.

## Partition key

The `id` field serves as both the document ID and partition key in the `__MigrationHistory` container. This means each migration maps to exactly one record, and reads/writes are efficient single-partition operations.
