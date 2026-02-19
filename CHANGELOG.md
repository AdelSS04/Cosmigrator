# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial release of Cosmigrator
- Core migration framework with `IMigration` interface
- `MigrationHost` static entry point for simplified integration
- `MigrationRunner` for orchestrating migrations (run, rollback, status)
- `MigrationHistory` for tracking applied migrations in `__MigrationHistory` container
- `MigrationDiscovery` for reflection-based migration scanning
- `BulkOperationHelper` with retry logic for 429 throttling
- Support for multiple migration scenarios:
  - Document property additions/removals
  - Property renaming with data preservation
  - Unique key policy changes via container recreation
  - Composite index additions
- System.Text.Json support (no Newtonsoft.Json dependency)
- `DefaultValue` property on `IMigration` interface for property defaults
- Serverless Cosmos DB compatibility
- Terraform-first approach (no auto-creation of containers)
- Comprehensive XML documentation
- Sample migration project with 5 example scenarios

### Changed
- N/A (initial release)

### Deprecated
- N/A (initial release)

### Removed
- N/A (initial release)

### Fixed
- N/A (initial release)

### Security
- N/A (initial release)

## [1.0.0] - TBD

Initial public release.

[Unreleased]: https://github.com/AdelSS04/Cosmigrator/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/AdelSS04/Cosmigrator/releases/tag/v1.0.0
