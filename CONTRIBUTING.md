# Contributing to Cosmigrator

First off, thank you for considering contributing to Cosmigrator! It's people like you that make this project such a great tool.

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues to avoid duplicates. When you create a bug report, include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples** (code snippets, configuration files)
- **Describe the behavior you observed and what you expected**
- **Include logs** (with sensitive data redacted)
- **Environment details** (.NET version, OS, Cosmos DB tier)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide a step-by-step description** of the suggested enhancement
- **Provide specific examples** to demonstrate the steps
- **Describe the current behavior** and **explain the behavior you'd like to see**
- **Explain why this enhancement would be useful**

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Follow the coding style** (see .editorconfig)
3. **Add tests** if you're adding functionality
4. **Update documentation** if you're changing behavior
5. **Ensure all tests pass** (`dotnet test`)
6. **Update CHANGELOG.md** under "Unreleased" section
7. **Reference the issue** you're fixing (e.g., "Fixes #123")

## Development Setup

### Prerequisites

- .NET 8.0 SDK or later
- Azure Cosmos DB account (or emulator) for integration tests
- Your favorite IDE (Visual Studio, VS Code, or Rider)

### Building the Project

```bash
# Clone your fork
git clone https://github.com/AdelSS04/Cosmigrator.git
cd Cosmigrator

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test
```

### Project Structure

```
├── Cosmigrator/          # Core library (main functionality)
├── Cosmigrator.Sample/        # Sample migration project
├── Cosmigrator.Tests/         # Unit and integration tests
└── .github/                        # GitHub Actions workflows
```

## Coding Guidelines

### C# Style Guide

- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and small
- Use `async`/`await` for all I/O operations

### Git Commit Messages

- Use the present tense ("Add feature" not "Added feature")
- Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit the first line to 72 characters
- Reference issues and pull requests liberally after the first line

Example:
```
Add retry logic for transient Cosmos DB errors

- Implement exponential backoff with jitter
- Add configurable max retry attempts
- Update BulkOperationHelper with retry policy

Fixes #42
```

### Testing

- Write unit tests for new functionality
- Ensure all tests pass before submitting PR
- Aim for high code coverage (>80%)
- Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`

Example:
```csharp
[Fact]
public async Task BulkUpsertAsync_WithThrottling_RetriesWithBackoff()
{
    // Arrange
    // Act
    // Assert
}
```

## Documentation

- Update README.md if you change public APIs
- Add XML comments for public classes and methods
- Update migration examples if behavior changes
- Keep CHANGELOG.md updated

## Release Process

Maintainers follow this process for releases:

1. Update CHANGELOG.md with release date
2. Update version in .csproj files
3. Create and push a version tag (`v1.0.0`)
4. GitHub Actions builds and publishes NuGet packages
5. Create GitHub release with changelog

## Questions?

Feel free to open a discussion or reach out to the maintainers!

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
