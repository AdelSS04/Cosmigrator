# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.4] - 2026-02-20

### Added
- `ReadDocumentsAsync(Container, string sql)` method in `BulkOperationHelper` for server-side filtered queries with custom SQL
- New sample migration `AddCompositeIndexToUsers` demonstrating composite index policies
- Seed data file (`seed-data.json`) for the sample project

### Changed
- Refactored `BulkOperationHelper` to use primary constructor (C# 12)
- Refactored `MigrationHistory` to use primary constructor (C# 12)
- Refactored `MigrationRunner.InitializeAsync` to expression-bodied method
- Updated `MigrationDiscovery` to use collection expression syntax
- Updated `MigrationHistory.GetAppliedMigrationsAsync` to use collection expression
- Improved logging in `ReadDocumentsAsync` to include the SQL query string
- Refactored sample migrations to use targeted queries instead of full-container scans
- Rewrote README to accurately reflect the current codebase and API surface

## [1.0.3] - 2026-02-19

### Added
- Multi-target framework support for .NET 8, .NET 9, and .NET 10
- Central Package Management via `Directory.Packages.props`
- Per-TFM dependency resolution with `VersionOverride` for Microsoft.Extensions packages
- System.Text.Json serializer configuration in `CosmosClientOptions` (`UseSystemTextJsonSerializerWithOptions`)
- Centralized `AzureCosmosDisableNewtonsoftJsonCheck` in `Directory.Build.props`

### Changed
- Default target framework set to .NET 10
- Upgraded `Microsoft.Azure.Cosmos` from 3.x to 3.57.0
- Upgraded `Microsoft.Extensions.*` baseline versions to 10.0.0
- Moved all package versions from individual `.csproj` files to `Directory.Packages.props`
- Updated `global.json` SDK to 10.0.100 with `latestPatch` roll-forward policy

### Removed
- Per-project `AzureCosmosDisableNewtonsoftJsonCheck` property (now centralized)
- Floating version ranges (`*`) in package references (replaced by Central Package Management)

## [1.0.0] - TBD

Initial public release.

[Unreleased]: https://github.com/AdelSS04/Cosmigrator/compare/v1.0.4...HEAD
[1.0.4]: https://github.com/AdelSS04/Cosmigrator/compare/v1.0.3...v1.0.4
[1.0.3]: https://github.com/AdelSS04/Cosmigrator/compare/v1.0.0...v1.0.3
[1.0.0]: https://github.com/AdelSS04/Cosmigrator/releases/tag/v1.0.0
