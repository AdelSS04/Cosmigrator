---
title: MigrationDiscovery
sidebar_position: 6
---

# MigrationDiscovery

Discovers all classes implementing `IMigration` via reflection and returns them sorted by `Id`.

```csharp
namespace Cosmigrator;

public static class MigrationDiscovery
{
    public static List<IMigration> DiscoverAll(params Assembly[] assemblies);
}
```

## Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `DiscoverAll(params Assembly[])` | `List<IMigration>` | Scans assemblies for `IMigration` implementations, instantiates them, returns sorted by `Id` |

## DiscoverAll

```csharp
var migrations = MigrationDiscovery.DiscoverAll(Assembly.GetExecutingAssembly());
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `assemblies` | `params Assembly[]` | Assemblies to scan. If empty, defaults to `Assembly.GetEntryAssembly()` |

### How it works

1. Selects target assemblies — provided assemblies, or the entry assembly if none specified
2. Scans all types using `assembly.GetTypes()`
3. Filters to types that:
   - Implement `IMigration`
   - Are not abstract
   - Are not interfaces
4. Instantiates each via `Activator.CreateInstance`
5. Sorts by `IMigration.Id` (lexicographic)
6. Returns as `List<IMigration>`

### Requirements for discovered types

- Must have a **parameterless constructor** (since `Activator.CreateInstance` is used)
- Must be **concrete** (not abstract, not interface)
- Must implement `IMigration`

### Example

Given these classes in an assembly:

```csharp
public class _20250101_000001_AddAge : IMigration { ... }
public class _20250101_000002_RemoveName : IMigration { ... }
public abstract class BaseMigration : IMigration { ... }  // skipped
```

`DiscoverAll` returns:
1. `_20250101_000001_AddAge` (Id: `"20250101_000001"`)
2. `_20250101_000002_RemoveName` (Id: `"20250101_000002"`)

`BaseMigration` is excluded because it's abstract.
