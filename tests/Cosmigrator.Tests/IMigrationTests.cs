using FluentAssertions;
using Microsoft.Azure.Cosmos;

namespace Cosmigrator.Tests;

public class IMigrationTests
{
    [Fact]
    public void DefaultValue_ShouldReturnNullByDefault()
    {
        // Arrange
        IMigration migration = new TestMigrationWithoutDefaultValue();

        // Act
        var defaultValue = migration.DefaultValue;

        // Assert
        defaultValue.Should().BeNull();
    }

    [Fact]
    public void DefaultValue_CanBeOverridden()
    {
        // Arrange
        IMigration migration = new TestMigrationWithDefaultValue();

        // Act
        var defaultValue = migration.DefaultValue;

        // Assert
        defaultValue.Should().Be(42);
    }

    [Theory]
    [InlineData(0)]
    [InlineData("")]
    [InlineData("N/A")]
    public void DefaultValue_CanBeAnyType(object? expectedValue)
    {
        // Arrange
        IMigration migration = new TestMigrationWithCustomDefaultValue(expectedValue);

        // Act
        var actualValue = migration.DefaultValue;

        // Assert
        actualValue.Should().Be(expectedValue);
    }
}

// Test fixtures
public class TestMigrationWithoutDefaultValue : IMigration
{
    public string Id => "test";
    public string Name => "test";
    public string ContainerName => "test";
    public Task UpAsync(Container container, CosmosClient client) => Task.CompletedTask;
    public Task DownAsync(Container container, CosmosClient client) => Task.CompletedTask;
}

public class TestMigrationWithDefaultValue : IMigration
{
    public string Id => "test";
    public string Name => "test";
    public string ContainerName => "test";
    public object? DefaultValue => 42;
    public Task UpAsync(Container container, CosmosClient client) => Task.CompletedTask;
    public Task DownAsync(Container container, CosmosClient client) => Task.CompletedTask;
}

public class TestMigrationWithCustomDefaultValue : IMigration
{
    private readonly object? _defaultValue;

    public TestMigrationWithCustomDefaultValue() : this(null)
    {
    }

    public TestMigrationWithCustomDefaultValue(object? defaultValue)
    {
        _defaultValue = defaultValue;
    }

    public string Id => "test_custom";
    public string Name => "test";
    public string ContainerName => "test";
    public object? DefaultValue => _defaultValue;
    public Task UpAsync(Container container, CosmosClient client) => Task.CompletedTask;
    public Task DownAsync(Container container, CosmosClient client) => Task.CompletedTask;
}
