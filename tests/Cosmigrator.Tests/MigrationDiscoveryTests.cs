using FluentAssertions;
using Microsoft.Azure.Cosmos;

namespace Cosmigrator.Tests;

public class MigrationDiscoveryTests
{
    [Fact]
    public void DiscoverMigrations_ShouldReturnMigrationsInOrder()
    {
        var assembly = typeof(DiscoveryMigrationA).Assembly;

        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        migrations.Should().NotBeEmpty();
        migrations.Should().BeInAscendingOrder(m => m.Id);
    }

    [Fact]
    public void DiscoverMigrations_ShouldExcludeAbstractClasses()
    {
        var assembly = typeof(AbstractTestMigration).Assembly;

        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        migrations.Should().NotContain(m => m.GetType().IsAbstract);
    }

    [Fact]
    public void DiscoverMigrations_ShouldOnlyIncludeIMigrationImplementations()
    {
        var assembly = typeof(NotAMigration).Assembly;

        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        migrations.Should().AllSatisfy(m => m.Should().BeAssignableTo<IMigration>());
    }

    [Fact]
    public void DiscoverMigrations_ShouldHandleEmptyAssembly()
    {
        var assembly = typeof(string).Assembly;

        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        migrations.Should().BeEmpty();
    }

    [Fact]
    public void DiscoverMigrations_ShouldNotIncludeInterfaces()
    {
        var assembly = typeof(DiscoveryMigrationA).Assembly;

        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        migrations.Should().AllSatisfy(m => m.GetType().IsInterface.Should().BeFalse());
    }

    [Fact]
    public void DiscoverMigrations_ShouldReturnDistinctInstances()
    {
        var assembly = typeof(DiscoveryMigrationA).Assembly;

        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        migrations.Should().OnlyHaveUniqueItems(m => m.GetType());
    }

    [Fact]
    public void DiscoverMigrations_ShouldInstantiateConcreteMigrations()
    {
        var assembly = typeof(DiscoveryMigrationA).Assembly;

        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        migrations.Should().AllSatisfy(m =>
        {
            m.Id.Should().NotBeNullOrEmpty();
            m.Name.Should().NotBeNullOrEmpty();
            m.ContainerName.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public void DiscoverMigrations_WithMultipleAssemblies_ShouldScanAll()
    {
        var assembly1 = typeof(DiscoveryMigrationA).Assembly;
        var assembly2 = typeof(string).Assembly;

        var migrations = MigrationDiscovery.DiscoverAll(assembly1, assembly2);

        // Should still find migrations from the test assembly
        migrations.Should().NotBeEmpty();
    }

    [Fact]
    public void DiscoverMigrations_CalledTwice_ShouldReturnIndependentLists()
    {
        var assembly = typeof(DiscoveryMigrationA).Assembly;

        var migrations1 = MigrationDiscovery.DiscoverAll(assembly);
        var migrations2 = MigrationDiscovery.DiscoverAll(assembly);

        migrations1.Should().NotBeSameAs(migrations2);
        migrations1.Should().HaveSameCount(migrations2);
    }

    [Fact]
    public void DiscoverMigrations_ShouldIncludeMigrationsWithDefaultValue()
    {
        var assembly = typeof(TestMigrationWithDefaultValue).Assembly;

        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        migrations.Should().Contain(m => m.DefaultValue != null);
    }

    [Fact]
    public void DiscoverMigrations_ShouldNotIncludeNotAMigration()
    {
        var assembly = typeof(NotAMigration).Assembly;

        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        migrations.Should().NotContain(m => m.GetType() == typeof(NotAMigration));
    }

    [Fact]
    public void DiscoverMigrations_OrderingShouldBeLexicographic()
    {
        var assembly = typeof(DiscoveryMigrationA).Assembly;

        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        for (var i = 1; i < migrations.Count; i++)
        {
            string.Compare(migrations[i - 1].Id, migrations[i].Id, StringComparison.Ordinal)
                .Should().BeLessThanOrEqualTo(0);
        }
    }

    [Fact]
    public void DiscoverMigrations_ShouldNotIncludePrivateNestedTypes()
    {
        var assembly = typeof(DiscoveryMigrationA).Assembly;

        var migrations = MigrationDiscovery.DiscoverAll(assembly);

        // Private nested types should not be included because they can't be discovered
        // by Activator.CreateInstance without BindingFlags
        migrations.Should().AllSatisfy(m => m.GetType().IsPublic.Should().BeTrue("public types are discoverable"));
    }
}

// Test fixtures for discovery
public class DiscoveryMigrationA : IMigration
{
    public string Id => "20240101_000001";
    public string Name => "Discovery Test A";
    public string ContainerName => "TestContainer";
    public Task UpAsync(Container container, CosmosClient client) => Task.CompletedTask;
    public Task DownAsync(Container container, CosmosClient client) => Task.CompletedTask;
}

public class DiscoveryMigrationB : IMigration
{
    public string Id => "20240102_000001";
    public string Name => "Discovery Test B";
    public string ContainerName => "TestContainer";
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
