---
title: History Tracking
sidebar_position: 3
---

# History Tracking

Cosmigrator tracks all migration state in a dedicated Cosmos DB container called `__MigrationHistory`.

## The history container

You must create this container before running any migrations:

| Property | Value |
|----------|-------|
| Container name | `__MigrationHistory` |
| Partition key | `/id` |

Each document in this container is a `MigrationRecord`:

```json
{
  "id": "20250219_000001",
  "name": "AddEmailToUsers",
  "appliedAt": "2025-02-19T14:30:00.000Z",
  "status": "Applied"
}
```

## MigrationRecord fields

| Field | Type | Description |
|-------|------|-------------|
| `id` | `string` | Migration identifier (same as `IMigration.Id`). Also the partition key |
| `name` | `string` | Human-readable name (from `IMigration.Name`) |
| `appliedAt` | `DateTime` | UTC timestamp of when the migration was applied or rolled back |
| `status` | `MigrationStatus` | Either `Applied` or `RolledBack` |

## How records are managed

### On apply

When a migration's `UpAsync` completes successfully, `MigrationHistory.MarkAsAppliedAsync` upserts a record:

```csharp
var record = new MigrationRecord
{
    Id = migration.Id,
    Name = migration.Name,
    AppliedAt = DateTime.UtcNow,
    Status = MigrationStatus.Applied
};

await _container.UpsertItemAsync(record, new PartitionKey(record.Id));
```

### On rollback

When `DownAsync` completes, `MigrationHistory.MarkAsRolledBackAsync` reads the existing record, updates its status, and upserts:

```csharp
var response = await _container.ReadItemAsync<MigrationRecord>(
    migrationId, new PartitionKey(migrationId));

var record = response.Resource;
record.Status = MigrationStatus.RolledBack;
record.AppliedAt = DateTime.UtcNow; // updated to rollback time

await _container.UpsertItemAsync(record, new PartitionKey(record.Id));
```

If the record doesn't exist (e.g., manual deletion), the rollback logs a warning and continues.

## Querying status

Use the `status` CLI command to see all migrations and their state:

```bash
dotnet run -- status
```

Internally, `MigrationRunner.PrintStatusAsync` cross-references discovered migrations against history records:

- If a record exists → shows `Applied` or `RolledBack` with timestamp
- If no record exists → shows `Pending`
