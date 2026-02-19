# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/AdelSS04/Cosmigrator/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/AdelSS04/Cosmigrator/releases/tag/v1.0.0
