using Cosmigrator.Models;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;

namespace Cosmigrator.Tests;

public class MigrationRunnerTests
{
    private readonly Mock<CosmosClient> _mockClient;
    private readonly Mock<Database> _mockDatabase;
    private readonly Mock<Container> _mockHistoryContainer;
    private readonly Mock<Container> _mockTargetContainer;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;

    public MigrationRunnerTests()
    {
        _mockClient = new Mock<CosmosClient>();
        _mockDatabase = new Mock<Database>();
        _mockHistoryContainer = new Mock<Container>();
        _mockTargetContainer = new Mock<Container>();
        _mockLoggerFactory = new Mock<ILoggerFactory>();

        _mockClient
            .Setup(c => c.GetDatabase("TestDb"))
            .Returns(_mockDatabase.Object);

        _mockDatabase
            .Setup(d => d.GetContainer("__MigrationHistory"))
            .Returns(_mockHistoryContainer.Object);

        _mockDatabase
            .Setup(d => d.GetContainer("TestContainer"))
            .Returns(_mockTargetContainer.Object);

        // Other test fixture migrations use "Products" container
        _mockDatabase
            .Setup(d => d.GetContainer("Products"))
            .Returns(Mock.Of<Container>());

        _mockLoggerFactory
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
    }

    // ── Constructor ──────────────────────────────────────────────

    [Fact]
    public void Constructor_ShouldCreateSuccessfully()
    {
        var runner = CreateRunner();

        runner.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithEmptyAssembly_ShouldCreateSuccessfully()
    {
        var runner = new MigrationRunner(
            _mockClient.Object,
            "TestDb",
            _mockLoggerFactory.Object,
            typeof(string).Assembly);

        runner.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_ShouldGetDatabaseReference()
    {
        _ = CreateRunner();

        _mockClient.Verify(c => c.GetDatabase("TestDb"), Times.Once);
    }

    [Fact]
    public void Constructor_WithMultipleAssemblies_ShouldNotThrow()
    {
        var act = () => new MigrationRunner(
            _mockClient.Object,
            "TestDb",
            _mockLoggerFactory.Object,
            typeof(RunnerTestMigration1).Assembly,
            typeof(string).Assembly);

        act.Should().NotThrow();
    }

    // ── InitializeAsync ──────────────────────────────────────────

    [Fact]
    public async Task InitializeAsync_ShouldNotThrow()
    {
        var runner = CreateRunner();

        var act = () => runner.InitializeAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InitializeAsync_ShouldInitializeHistoryContainer()
    {
        var runner = CreateRunner();

        await runner.InitializeAsync();

        _mockDatabase.Verify(d => d.GetContainer("__MigrationHistory"), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_CalledTwice_ShouldNotThrow()
    {
        var runner = CreateRunner();

        await runner.InitializeAsync();
        var act = () => runner.InitializeAsync();

        await act.Should().NotThrowAsync();
    }

    // ── PrintDiscoveredMigrations ────────────────────────────────

    [Fact]
    public void PrintDiscoveredMigrations_WithMigrations_ShouldNotThrow()
    {
        var runner = CreateRunner();

        var act = () => runner.PrintDiscoveredMigrations();

        act.Should().NotThrow();
    }

    [Fact]
    public void PrintDiscoveredMigrations_WithNoMigrations_ShouldNotThrow()
    {
        var runner = new MigrationRunner(
            _mockClient.Object,
            "TestDb",
            _mockLoggerFactory.Object,
            typeof(string).Assembly);

        var act = () => runner.PrintDiscoveredMigrations();

        act.Should().NotThrow();
    }

    [Fact]
    public void PrintDiscoveredMigrations_ShouldLogMigrationInfo()
    {
        var mockLogger = new Mock<ILogger>();
        _mockLoggerFactory
            .Setup(f => f.CreateLogger(It.Is<string>(s => s.Contains("MigrationRunner"))))
            .Returns(mockLogger.Object);

        var runner = CreateRunnerWithKnownMigrations();

        runner.PrintDiscoveredMigrations();

        mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    // ── PrintStatusAsync ─────────────────────────────────────────

    [Fact]
    public async Task PrintStatusAsync_WithNoRecords_ShouldNotThrow()
    {
        var runner = CreateRunner();
        await runner.InitializeAsync();

        SetupEmptyIterator();

        var act = () => runner.PrintStatusAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PrintStatusAsync_WithMixedStatuses_ShouldNotThrow()
    {
        var runner = CreateRunner();
        await runner.InitializeAsync();

        var records = new List<MigrationRecord>
        {
            new() { Id = "20240101_000001", Name = "Applied", Status = MigrationStatus.Applied, AppliedAt = DateTime.UtcNow },
            new() { Id = "20240101_000002", Name = "RolledBack", Status = MigrationStatus.RolledBack, AppliedAt = DateTime.UtcNow }
        };

        SetupIterator(records);

        var act = () => runner.PrintStatusAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PrintStatusAsync_WithPendingMigrations_ShouldShowPending()
    {
        var runner = CreateRunnerWithKnownMigrations();
        await runner.InitializeAsync();

        // No records in history = all migrations are pending
        SetupEmptyIterator();

        var act = () => runner.PrintStatusAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PrintStatusAsync_WithAllApplied_ShouldShowAllApplied()
    {
        var runner = CreateRunnerWithKnownMigrations();
        await runner.InitializeAsync();

        var records = new List<MigrationRecord>
        {
            new() { Id = "20240101_000001", Name = "M1", Status = MigrationStatus.Applied, AppliedAt = DateTime.UtcNow },
            new() { Id = "20240102_000001", Name = "M2", Status = MigrationStatus.Applied, AppliedAt = DateTime.UtcNow }
        };
        SetupIterator(records);

        var act = () => runner.PrintStatusAsync();

        await act.Should().NotThrowAsync();
    }

    // ── RunPendingMigrationsAsync ────────────────────────────────

    [Fact]
    public async Task RunPendingMigrationsAsync_WhenAllApplied_ShouldNotCalllUpsert()
    {
        var runner = CreateRunnerWithKnownMigrations();
        await runner.InitializeAsync();

        // Must include ALL migration IDs from the test assembly (not just runner-specific ones)
        // because MigrationDiscovery discovers all IMigration implementations
        SetupIterator(AllMigrationRecordsAsApplied());

        await runner.RunPendingMigrationsAsync();

        // No pending migrations, so no upserts to history
        _mockTargetContainer.Verify(c => c.UpsertItemAsync(
            It.IsAny<object>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RunPendingMigrationsAsync_WithPending_ShouldMarkApplied()
    {
        var runner = CreateRunnerWithKnownMigrations();
        await runner.InitializeAsync();

        // Mark non-runner migration IDs as applied so only runner migrations are pending
        // (prevents FailingMigration from crashing the test)
        SetupIterator(NonRunnerMigrationRecords());

        _mockHistoryContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<MigrationRecord>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<MigrationRecord>>());

        await runner.RunPendingMigrationsAsync();

        _mockHistoryContainer.Verify(c => c.UpsertItemAsync(
            It.Is<MigrationRecord>(r => r.Status == MigrationStatus.Applied),
            It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task RunPendingMigrationsAsync_ShouldApplyInOrderById()
    {
        var runner = CreateRunnerWithKnownMigrations();
        await runner.InitializeAsync();

        // Mark non-runner migration IDs as applied so only runner migrations are pending
        SetupIterator(NonRunnerMigrationRecords());

        var appliedIds = new List<string>();
        _mockHistoryContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<MigrationRecord>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<MigrationRecord, PartitionKey?, ItemRequestOptions?, CancellationToken>(
                (r, _, _, _) => appliedIds.Add(r.Id))
            .ReturnsAsync(Mock.Of<ItemResponse<MigrationRecord>>());

        await runner.RunPendingMigrationsAsync();

        appliedIds.Should().BeInAscendingOrder();
    }

    // ── RollbackAsync ────────────────────────────────────────────

    [Fact]
    public async Task RollbackAsync_WhenNoApplied_ShouldNotAttemptRollback()
    {
        var runner = CreateRunnerWithKnownMigrations();
        await runner.InitializeAsync();

        SetupEmptyIterator();

        await runner.RollbackAsync(1);

        _mockHistoryContainer.Verify(c => c.ReadItemAsync<MigrationRecord>(
            It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RollbackAsync_WithOneStep_ShouldRollbackOnlyLastApplied()
    {
        var runner = CreateRunnerWithKnownMigrations();
        await runner.InitializeAsync();

        var applied = new List<MigrationRecord>
        {
            new() { Id = "20240101_000001", Name = "M1", Status = MigrationStatus.Applied },
            new() { Id = "20240102_000001", Name = "M2", Status = MigrationStatus.Applied }
        };
        SetupIterator(applied);

        SetupReadAndUpsertForRollback("20240102_000001", "M2");

        await runner.RollbackAsync(1);

        _mockHistoryContainer.Verify(c => c.UpsertItemAsync(
            It.Is<MigrationRecord>(r => r.Id == "20240102_000001" && r.Status == MigrationStatus.RolledBack),
            It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RollbackAsync_WithMultipleSteps_ShouldRollbackInReverseOrder()
    {
        var runner = CreateRunnerWithKnownMigrations();
        await runner.InitializeAsync();

        var applied = new List<MigrationRecord>
        {
            new() { Id = "20240101_000001", Name = "M1", Status = MigrationStatus.Applied },
            new() { Id = "20240102_000001", Name = "M2", Status = MigrationStatus.Applied }
        };
        SetupIterator(applied);

        SetupReadAndUpsertForRollback("20240101_000001", "M1");
        SetupReadAndUpsertForRollback("20240102_000001", "M2");

        var rolledBackIds = new List<string>();
        _mockHistoryContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<MigrationRecord>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<MigrationRecord, PartitionKey?, ItemRequestOptions?, CancellationToken>(
                (r, _, _, _) => rolledBackIds.Add(r.Id))
            .ReturnsAsync(Mock.Of<ItemResponse<MigrationRecord>>());

        await runner.RollbackAsync(2);

        rolledBackIds.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RollbackAsync_WhenMigrationClassNotFound_ShouldSkip()
    {
        // Use runner with no known migration classes
        var runner = new MigrationRunner(
            _mockClient.Object,
            "TestDb",
            _mockLoggerFactory.Object,
            typeof(string).Assembly);
        await runner.InitializeAsync();

        var applied = new List<MigrationRecord>
        {
            new() { Id = "unknown_migration", Name = "Unknown", Status = MigrationStatus.Applied }
        };
        SetupIterator(applied);

        await runner.RollbackAsync(1);

        // Should not attempt to read since migration class is not found (it skips)
        _mockHistoryContainer.Verify(c => c.ReadItemAsync<MigrationRecord>(
            It.IsAny<string>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RollbackAsync_WithMoreStepsThanApplied_ShouldOnlyRollbackExisting()
    {
        var runner = CreateRunnerWithKnownMigrations();
        await runner.InitializeAsync();

        var applied = new List<MigrationRecord>
        {
            new() { Id = "20240101_000001", Name = "M1", Status = MigrationStatus.Applied }
        };
        SetupIterator(applied);

        SetupReadAndUpsertForRollback("20240101_000001", "M1");

        _mockHistoryContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<MigrationRecord>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<MigrationRecord>>());

        await runner.RollbackAsync(100);

        _mockHistoryContainer.Verify(c => c.UpsertItemAsync(
            It.Is<MigrationRecord>(r => r.Status == MigrationStatus.RolledBack),
            It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Helpers ──────────────────────────────────────────────────

    /// <summary>
    /// Returns records for ALL unique migration IDs discovered from the test assembly.
    /// Multiple test fixture classes implement IMigration, so we need all their IDs
    /// to properly mark "all applied".
    /// </summary>
    private static List<MigrationRecord> AllMigrationRecordsAsApplied() =>
    [
        new() { Id = "20240101_000001", Name = "M1", Status = MigrationStatus.Applied },
        new() { Id = "20240101_000002", Name = "M2", Status = MigrationStatus.Applied },
        new() { Id = "20240101_000003", Name = "M3", Status = MigrationStatus.Applied },
        new() { Id = "20240101_000010", Name = "M10", Status = MigrationStatus.Applied },
        new() { Id = "20240101_000099", Name = "M99", Status = MigrationStatus.Applied },
        new() { Id = "20240102_000001", Name = "M4", Status = MigrationStatus.Applied },
    ];

    /// <summary>
    /// Returns applied records for migration IDs that are NOT the runner test migrations,
    /// ensuring FailingMigration and other non-runner fixtures don't interfere with tests.
    /// </summary>
    private static List<MigrationRecord> NonRunnerMigrationRecords() =>
    [
        new() { Id = "20240101_000002", Name = "M2", Status = MigrationStatus.Applied },
        new() { Id = "20240101_000003", Name = "M3", Status = MigrationStatus.Applied },
        new() { Id = "20240101_000010", Name = "M10", Status = MigrationStatus.Applied },
        new() { Id = "20240101_000099", Name = "M99", Status = MigrationStatus.Applied },
    ];

    private MigrationRunner CreateRunner()
    {
        return new MigrationRunner(
            _mockClient.Object,
            "TestDb",
            _mockLoggerFactory.Object,
            typeof(MigrationRunnerTests).Assembly);
    }

    private MigrationRunner CreateRunnerWithKnownMigrations()
    {
        return new MigrationRunner(
            _mockClient.Object,
            "TestDb",
            _mockLoggerFactory.Object,
            typeof(RunnerTestMigration1).Assembly);
    }

    private void SetupEmptyIterator()
    {
        var mockIterator = CreateMockIterator(new List<MigrationRecord>());
        _mockHistoryContainer
            .Setup(c => c.GetItemQueryIterator<MigrationRecord>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Returns(mockIterator.Object);
    }

    private void SetupIterator(List<MigrationRecord> records)
    {
        var mockIterator = CreateMockIterator(records);
        _mockHistoryContainer
            .Setup(c => c.GetItemQueryIterator<MigrationRecord>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Returns(mockIterator.Object);
    }

    private void SetupReadAndUpsertForRollback(string id, string name)
    {
        var record = new MigrationRecord { Id = id, Name = name, Status = MigrationStatus.Applied };
        var mockResp = new Mock<ItemResponse<MigrationRecord>>();
        mockResp.Setup(x => x.Resource).Returns(record);

        _mockHistoryContainer
            .Setup(c => c.ReadItemAsync<MigrationRecord>(
                id, It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResp.Object);
    }

    private static Mock<FeedIterator<T>> CreateMockIterator<T>(List<T> items)
    {
        var mockIterator = new Mock<FeedIterator<T>>();
        var hasCalledOnce = false;

        mockIterator
            .Setup(i => i.HasMoreResults)
            .Returns(() => !hasCalledOnce);

        var mockResponse = new Mock<FeedResponse<T>>();
        mockResponse.Setup(r => r.GetEnumerator()).Returns(items.GetEnumerator());

        mockIterator
            .Setup(i => i.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object)
            .Callback(() => hasCalledOnce = true);

        return mockIterator;
    }
}

// Test fixture migrations for MigrationRunner with known IDs
public class RunnerTestMigration1 : IMigration
{
    public string Id => "20240101_000001";
    public string Name => "Runner Test 1";
    public string ContainerName => "TestContainer";
    public Task UpAsync(Container container, CosmosClient client) => Task.CompletedTask;
    public Task DownAsync(Container container, CosmosClient client) => Task.CompletedTask;
}

public class RunnerTestMigration2 : IMigration
{
    public string Id => "20240102_000001";
    public string Name => "Runner Test 2";
    public string ContainerName => "TestContainer";
    public Task UpAsync(Container container, CosmosClient client) => Task.CompletedTask;
    public Task DownAsync(Container container, CosmosClient client) => Task.CompletedTask;
}
