using FluentAssertions;
using Microsoft.Azure.Cosmos;

namespace Cosmigrator.Tests;

public class MigrationDiscoveryTests
{
    [Fact]
    public void DiscoverMigrations_ShouldReturnMigrationsInOrder()
    {
        // Arrange
        var assembly = typeof(TestMigration1).Assembly;

        // Act
        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        // Assert
        migrations.Should().NotBeEmpty();
        migrations.Should().BeInAscendingOrder(m => m.Id);
    }

    [Fact]
    public void DiscoverMigrations_ShouldExcludeAbstractClasses()
    {
        // Arrange
        var assembly = typeof(AbstractTestMigration).Assembly;

        // Act
        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        // Assert
        migrations.Should().NotContain(m => m.GetType().IsAbstract);
    }

    [Fact]
    public void DiscoverMigrations_ShouldOnlyIncludeIMigrationImplementations()
    {
        // Arrange
        var assembly = typeof(NotAMigration).Assembly;

        // Act
        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        // Assert
        migrations.Should().AllSatisfy(m => m.Should().BeAssignableTo<IMigration>());
    }

    [Fact]
    public void DiscoverMigrations_ShouldHandleEmptyAssembly()
    {
        // Arrange
        var assembly = typeof(string).Assembly; // System assembly with no migrations

        // Act
        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        // Assert
        migrations.Should().BeEmpty();
    }
}

// Test fixtures
public class TestMigration1 : IMigration
{
    public string Id => "20240101_000001";
    public string Name => "Test1";
    public string ContainerName => "Test";
    public Task UpAsync(Container container, CosmosClient client) => Task.CompletedTask;
    public Task DownAsync(Container container, CosmosClient client) => Task.CompletedTask;
}

public class TestMigration2 : IMigration
{
    public string Id => "20240101_000002";
    public string Name => "Test2";
    public string ContainerName => "Test";
    public Task UpAsync(Container container, CosmosClient client) => Task.CompletedTask;
    public Task DownAsync(Container container, CosmosClient client) => Task.CompletedTask;
}

public abstract class AbstractTestMigration : IMigration
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string ContainerName { get; }
    public abstract Task UpAsync(Container container, CosmosClient client);
    public abstract Task DownAsync(Container container, CosmosClient client);
}

public class NotAMigration
{
    public string Id => "not_a_migration";
}
