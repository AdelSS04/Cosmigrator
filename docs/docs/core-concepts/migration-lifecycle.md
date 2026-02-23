---
title: Migration Lifecycle
sidebar_position: 2
---

# Migration Lifecycle

Every migration goes through a defined lifecycle tracked in the `__MigrationHistory` container.

## States

```
  Discovered  ──▶  Applied  ──▶  RolledBack
  (no record)     (record)      (record updated)
```

| State | History record | Meaning |
|-------|---------------|---------|
| **Pending** | No record exists | Migration class found in assembly but not yet applied |
| **Applied** | `status: "Applied"` | `UpAsync` completed successfully |
| **RolledBack** | `status: "RolledBack"` | `DownAsync` completed successfully after a rollback |

## Forward migration

When you run `dotnet run` (or `dotnet run -- migrate`):

1. All `IMigration` classes are discovered via reflection
2. Applied migration IDs are loaded from `__MigrationHistory`
3. Migrations not in the applied set are **pending**
4. Pending migrations execute in `Id` order
5. After each successful `UpAsync`, a `MigrationRecord` is upserted with status `Applied`

```csharp
// What MigrationRunner does internally:
var applied = await _history.GetAppliedMigrationsAsync();
var appliedIds = new HashSet<string>(applied.Select(a => a.Id));

var pending = _migrations
    .Where(m => !appliedIds.Contains(m.Id))
    .OrderBy(m => m.Id)
    .ToList();

foreach (var migration in pending)
{
    var container = _database.GetContainer(migration.ContainerName);
    await migration.UpAsync(container, _client);
    await _history.MarkAsAppliedAsync(migration);
}
```

## Rollback

When you run `dotnet run -- rollback --steps N`:

1. Applied migrations are loaded and sorted in reverse `Id` order
2. The last N are selected for rollback
3. For each, `DownAsync` is called, then the record is updated to `RolledBack`

```csharp
// What MigrationRunner does internally:
var toRollback = applied
    .OrderByDescending(a => a.Id)
    .Take(steps)
    .ToList();

foreach (var record in toRollback)
{
    var migration = _migrations.FirstOrDefault(m => m.Id == record.Id);
    var container = _database.GetContainer(migration.ContainerName);
    await migration.DownAsync(container, _client);
    await _history.MarkAsRolledBackAsync(migration.Id);
}
```

## Re-applying after rollback

A rolled-back migration becomes **pending** again. Running `dotnet run` will re-apply it because `GetAppliedMigrationsAsync` only returns records with status `Applied`.

## Failure handling

If `UpAsync` or `DownAsync` throws an exception:

- The error is logged
- No history record is written (for forward) or the existing record stays as `Applied` (for rollback)
- The process exits with code `1`
- No further migrations are attempted

This fail-fast approach ensures you never end up in a half-migrated state without knowing about it.

## Id convention

Use the format `YYYYMMDD_NNNNNN`:

```
20250219_000001  — first migration on Feb 19, 2025
20250219_000002  — second migration same day
20250301_000001  — first migration on Mar 1, 2025
```

Migrations are sorted lexicographically by `Id`, so this format guarantees chronological order.
