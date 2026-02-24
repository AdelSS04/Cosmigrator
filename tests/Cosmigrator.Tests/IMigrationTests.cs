using FluentAssertions;
using Microsoft.Azure.Cosmos;

namespace Cosmigrator.Tests;

public class IMigrationTests
{
    [Fact]
    public void DefaultValue_ShouldReturnNullByDefault()
    {
        IMigration migration = new TestMigrationWithoutDefaultValue();

        migration.DefaultValue.Should().BeNull();
    }

    [Fact]
    public void DefaultValue_CanBeOverridden()
    {
        IMigration migration = new TestMigrationWithDefaultValue();

        migration.DefaultValue.Should().Be(42);
    }

    [Theory]
    [InlineData(0)]
    [InlineData("")]
    [InlineData("N/A")]
    public void DefaultValue_CanBeAnyType(object? expectedValue)
    {
        IMigration migration = new TestMigrationWithCustomDefaultValue(expectedValue);

        migration.DefaultValue.Should().Be(expectedValue);
    }

    [Fact]
    public void DefaultValue_CanBeComplexObject()
    {
        var complexValue = new Dictionary<string, object> { ["key"] = "value" };
        IMigration migration = new TestMigrationWithCustomDefaultValue(complexValue);

        migration.DefaultValue.Should().BeSameAs(complexValue);
    }

    [Fact]
    public void DefaultValue_CanBeGuid()
    {
        var guid = Guid.NewGuid();
        IMigration migration = new TestMigrationWithCustomDefaultValue(guid);

        migration.DefaultValue.Should().Be(guid);
    }

    [Fact]
    public void DefaultValue_CanBeBooleanFalse()
    {
        IMigration migration = new TestMigrationWithCustomDefaultValue(false);

        migration.DefaultValue.Should().Be(false);
    }

    [Fact]
    public void DefaultValue_CanBeEmptyList()
    {
        var emptyList = new List<string>();
        IMigration migration = new TestMigrationWithCustomDefaultValue(emptyList);

        migration.DefaultValue.Should().BeSameAs(emptyList);
    }

    [Fact]
    public void Migration_ShouldExposeId()
    {
        IMigration migration = new TestMigrationWithoutDefaultValue();

        migration.Id.Should().Be("20240101_000001");
    }

    [Fact]
    public void Migration_ShouldExposeName()
    {
        IMigration migration = new TestMigrationWithoutDefaultValue();

        migration.Name.Should().Be("Test migration");
    }

    [Fact]
    public void Migration_ShouldExposeContainerName()
    {
        IMigration migration = new TestMigrationWithoutDefaultValue();

        migration.ContainerName.Should().Be("TestContainer");
    }

    [Fact]
    public async Task UpAsync_ShouldBeCallable()
    {
        IMigration migration = new TestMigrationWithoutDefaultValue();

        var act = () => migration.UpAsync(null!, null!);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task DownAsync_ShouldBeCallable()
    {
        IMigration migration = new TestMigrationWithoutDefaultValue();

        var act = () => migration.DownAsync(null!, null!);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void Migration_ShouldBeAssignableToIMigration()
    {
        var migration = new TestMigrationWithoutDefaultValue();

        migration.Should().BeAssignableTo<IMigration>();
    }

    [Fact]
    public async Task UpAsync_TrackingMigration_ShouldRecordCall()
    {
        var migration = new TrackingMigration();

        await migration.UpAsync(null!, null!);

        migration.UpCalled.Should().BeTrue();
        migration.DownCalled.Should().BeFalse();
    }

    [Fact]
    public async Task DownAsync_TrackingMigration_ShouldRecordCall()
    {
        var migration = new TrackingMigration();

        await migration.DownAsync(null!, null!);

        migration.DownCalled.Should().BeTrue();
        migration.UpCalled.Should().BeFalse();
    }

    [Fact]
    public async Task FailingMigration_UpAsync_ShouldThrow()
    {
        IMigration migration = new FailingMigration();

        var act = () => migration.UpAsync(null!, null!);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Migration failed");
    }

    [Fact]
    public async Task FailingMigration_DownAsync_ShouldThrow()
    {
        IMigration migration = new FailingMigration();

        var act = () => migration.DownAsync(null!, null!);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Rollback failed");
    }
}

// Test fixtures
public class TestMigrationWithoutDefaultValue : IMigration
{
    public string Id => "20240101_000001";
    public string Name => "Test migration";
    public string ContainerName => "TestContainer";
    public Task UpAsync(Container container, CosmosClient client) => Task.CompletedTask;
    public Task DownAsync(Container container, CosmosClient client) => Task.CompletedTask;
}

public class TestMigrationWithDefaultValue : IMigration
{
    public string Id => "20240101_000002";
    public string Name => "Test with default";
    public string ContainerName => "TestContainer";
    public object? DefaultValue => 42;
    public Task UpAsync(Container container, CosmosClient client) => Task.CompletedTask;
    public Task DownAsync(Container container, CosmosClient client) => Task.CompletedTask;
}

public class TestMigrationWithCustomDefaultValue : IMigration
{
    private readonly object? _defaultValue;

    public TestMigrationWithCustomDefaultValue() : this(null) { }

    public TestMigrationWithCustomDefaultValue(object? defaultValue)
    {
        _defaultValue = defaultValue;
    }

    public string Id => "20240101_000003";
    public string Name => "Test custom default";
    public string ContainerName => "TestContainer";
    public object? DefaultValue => _defaultValue;
    public Task UpAsync(Container container, CosmosClient client) => Task.CompletedTask;
    public Task DownAsync(Container container, CosmosClient client) => Task.CompletedTask;
}

public class TrackingMigration : IMigration
{
    public bool UpCalled { get; private set; }
    public bool DownCalled { get; private set; }

    public string Id => "20240101_000010";
    public string Name => "Tracking migration";
    public string ContainerName => "TestContainer";

    public Task UpAsync(Container container, CosmosClient client)
    {
        UpCalled = true;
        return Task.CompletedTask;
    }

    public Task DownAsync(Container container, CosmosClient client)
    {
        DownCalled = true;
        return Task.CompletedTask;
    }
}

public class FailingMigration : IMigration
{
    public string Id => "20240101_000099";
    public string Name => "Failing migration";
    public string ContainerName => "TestContainer";

    public Task UpAsync(Container container, CosmosClient client)
        => throw new InvalidOperationException("Migration failed");

    public Task DownAsync(Container container, CosmosClient client)
        => throw new InvalidOperationException("Rollback failed");
}
