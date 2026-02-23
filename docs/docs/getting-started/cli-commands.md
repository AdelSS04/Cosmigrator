---
title: CLI Commands
sidebar_position: 3
---

# CLI Commands

Cosmigrator parses the first CLI argument as a command. If no argument is provided, it defaults to `migrate`.

## migrate

Applies all pending migrations in `Id` order.

```bash
dotnet run
# or explicitly:
dotnet run -- migrate
```

For each pending migration:
1. Gets the target container via `migration.ContainerName`
2. Calls `migration.UpAsync(container, client)`
3. Records the migration in `__MigrationHistory` with status `Applied`

Exits with code `0` on success, `1` on failure. If a migration fails, execution stops immediately — no further migrations are applied.

## rollback

Undoes the last N applied migrations in reverse order.

```bash
# Roll back the last migration
dotnet run -- rollback

# Roll back the last 3 migrations
dotnet run -- rollback --steps 3
```

For each migration being rolled back:
1. Finds the `IMigration` class matching the history record
2. Calls `migration.DownAsync(container, client)`
3. Updates the history record to status `RolledBack`

If a migration class is not found in the assembly (e.g., deleted code), that rollback is skipped with a warning.

## status

Prints the status of every discovered migration.

```bash
dotnet run -- status
```

Output shows each migration with its current status and timestamp:

```
[20250219_000001] AddEmailToUsers - Applied at 2025-02-19 14:30:00 UTC
[20250219_000002] RemoveMiddleName - Pending
```

Possible statuses: `Applied`, `RolledBack`, `Pending`.

## list

Lists all discovered migration classes from the assembly.

```bash
dotnet run -- list
```

Output:

```
[20250219_000001] AddEmailToUsers -> Users
[20250219_000002] RemoveMiddleName -> Users
Total: 2 migration(s) discovered
```

## Exit codes

| Code | Meaning |
|------|---------|
| `0` | Success |
| `1` | Failure — a migration threw an exception, or an unknown command was used |
