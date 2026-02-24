using Cosmigrator.Models;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;

namespace Cosmigrator.Tests;

public class MigrationHistoryTests
{
    private readonly Mock<Database> _mockDatabase;
    private readonly Mock<Container> _mockContainer;
    private readonly Mock<ILogger<MigrationHistory>> _mockLogger;
    private readonly MigrationHistory _history;

    public MigrationHistoryTests()
    {
        _mockDatabase = new Mock<Database>();
        _mockContainer = new Mock<Container>();
        _mockLogger = new Mock<ILogger<MigrationHistory>>();

        _mockDatabase
            .Setup(d => d.GetContainer("__MigrationHistory"))
            .Returns(_mockContainer.Object);

        _history = new MigrationHistory(_mockDatabase.Object, _mockLogger.Object);
    }

    // ── InitializeAsync ──────────────────────────────────────────

    [Fact]
    public async Task InitializeAsync_ShouldGetContainerReference()
    {
        await _history.InitializeAsync();

        _mockDatabase.Verify(d => d.GetContainer("__MigrationHistory"), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_ShouldCompleteSuccessfully()
    {
        var act = () => _history.InitializeAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task InitializeAsync_CalledMultipleTimes_ShouldNotThrow()
    {
        await _history.InitializeAsync();
        var act = () => _history.InitializeAsync();

        await act.Should().NotThrowAsync();
    }

    // ── EnsureInitialized ────────────────────────────────────────

    [Fact]
    public async Task GetAllRecordsAsync_BeforeInitialize_ShouldThrowInvalidOperationException()
    {
        var act = () => _history.GetAllRecordsAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*initialized*");
    }

    [Fact]
    public async Task GetAppliedMigrationsAsync_BeforeInitialize_ShouldThrowInvalidOperationException()
    {
        var act = () => _history.GetAppliedMigrationsAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*initialized*");
    }

    [Fact]
    public async Task MarkAsAppliedAsync_BeforeInitialize_ShouldThrowInvalidOperationException()
    {
        var migration = new Mock<IMigration>().Object;

        var act = () => _history.MarkAsAppliedAsync(migration);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*initialized*");
    }

    [Fact]
    public async Task MarkAsRolledBackAsync_BeforeInitialize_ShouldThrowInvalidOperationException()
    {
        var act = () => _history.MarkAsRolledBackAsync("test_id");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*initialized*");
    }

    // ── GetAllRecordsAsync ───────────────────────────────────────

    [Fact]
    public async Task GetAllRecordsAsync_WhenNoRecords_ShouldReturnEmptyList()
    {
        await _history.InitializeAsync();

        var mockIterator = CreateMockIterator(new List<MigrationRecord>());
        _mockContainer
            .Setup(c => c.GetItemQueryIterator<MigrationRecord>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Returns(mockIterator.Object);

        var records = await _history.GetAllRecordsAsync();

        records.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllRecordsAsync_WhenRecordsExist_ShouldReturnAll()
    {
        await _history.InitializeAsync();

        var expectedRecords = new List<MigrationRecord>
        {
            new() { Id = "20240101_000001", Name = "Migration1", Status = MigrationStatus.Applied },
            new() { Id = "20240102_000001", Name = "Migration2", Status = MigrationStatus.RolledBack }
        };

        var mockIterator = CreateMockIterator(expectedRecords);
        _mockContainer
            .Setup(c => c.GetItemQueryIterator<MigrationRecord>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Returns(mockIterator.Object);

        var records = await _history.GetAllRecordsAsync();

        records.Should().HaveCount(2);
        records[0].Id.Should().Be("20240101_000001");
        records[1].Id.Should().Be("20240102_000001");
    }

    // ── GetAppliedMigrationsAsync ────────────────────────────────

    [Fact]
    public async Task GetAppliedMigrationsAsync_ShouldReturnOnlyApplied()
    {
        await _history.InitializeAsync();

        var allRecords = new List<MigrationRecord>
        {
            new() { Id = "20240101_000001", Name = "Applied", Status = MigrationStatus.Applied },
            new() { Id = "20240102_000001", Name = "RolledBack", Status = MigrationStatus.RolledBack },
            new() { Id = "20240103_000001", Name = "Applied2", Status = MigrationStatus.Applied }
        };

        var mockIterator = CreateMockIterator(allRecords);
        _mockContainer
            .Setup(c => c.GetItemQueryIterator<MigrationRecord>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Returns(mockIterator.Object);

        var applied = await _history.GetAppliedMigrationsAsync();

        applied.Should().HaveCount(2);
        applied.Should().OnlyContain(r => r.Status == MigrationStatus.Applied);
    }

    [Fact]
    public async Task GetAppliedMigrationsAsync_ShouldReturnSortedById()
    {
        await _history.InitializeAsync();

        var allRecords = new List<MigrationRecord>
        {
            new() { Id = "20240103_000001", Name = "Third", Status = MigrationStatus.Applied },
            new() { Id = "20240101_000001", Name = "First", Status = MigrationStatus.Applied },
            new() { Id = "20240102_000001", Name = "Second", Status = MigrationStatus.Applied }
        };

        var mockIterator = CreateMockIterator(allRecords);
        _mockContainer
            .Setup(c => c.GetItemQueryIterator<MigrationRecord>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Returns(mockIterator.Object);

        var applied = await _history.GetAppliedMigrationsAsync();

        applied.Should().BeInAscendingOrder(r => r.Id);
    }

    [Fact]
    public async Task GetAppliedMigrationsAsync_WhenAllRolledBack_ShouldReturnEmpty()
    {
        await _history.InitializeAsync();

        var allRecords = new List<MigrationRecord>
        {
            new() { Id = "20240101_000001", Name = "M1", Status = MigrationStatus.RolledBack },
            new() { Id = "20240102_000001", Name = "M2", Status = MigrationStatus.RolledBack }
        };

        var mockIterator = CreateMockIterator(allRecords);
        _mockContainer
            .Setup(c => c.GetItemQueryIterator<MigrationRecord>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Returns(mockIterator.Object);

        var applied = await _history.GetAppliedMigrationsAsync();

        applied.Should().BeEmpty();
    }

    // ── MarkAsAppliedAsync ───────────────────────────────────────

    [Fact]
    public async Task MarkAsAppliedAsync_ShouldUpsertRecordWithAppliedStatus()
    {
        await _history.InitializeAsync();

        var mockMigration = new Mock<IMigration>();
        mockMigration.Setup(m => m.Id).Returns("20240101_000001");
        mockMigration.Setup(m => m.Name).Returns("Test Migration");

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<MigrationRecord>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<MigrationRecord>>());

        await _history.MarkAsAppliedAsync(mockMigration.Object);

        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.Is<MigrationRecord>(r =>
                r.Id == "20240101_000001" &&
                r.Name == "Test Migration" &&
                r.Status == MigrationStatus.Applied),
            It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MarkAsAppliedAsync_ShouldSetAppliedAtToUtcNow()
    {
        await _history.InitializeAsync();

        var mockMigration = new Mock<IMigration>();
        mockMigration.Setup(m => m.Id).Returns("test");
        mockMigration.Setup(m => m.Name).Returns("test");

        MigrationRecord? capturedRecord = null;
        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<MigrationRecord>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .Callback<MigrationRecord, PartitionKey?, ItemRequestOptions?, CancellationToken>(
                (r, _, _, _) => capturedRecord = r)
            .ReturnsAsync(Mock.Of<ItemResponse<MigrationRecord>>());

        var before = DateTime.UtcNow;
        await _history.MarkAsAppliedAsync(mockMigration.Object);
        var after = DateTime.UtcNow;

        capturedRecord.Should().NotBeNull();
        capturedRecord!.AppliedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public async Task MarkAsAppliedAsync_ShouldUseIdAsPartitionKey()
    {
        await _history.InitializeAsync();

        var mockMigration = new Mock<IMigration>();
        mockMigration.Setup(m => m.Id).Returns("20240101_000001");
        mockMigration.Setup(m => m.Name).Returns("Test");

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<MigrationRecord>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<MigrationRecord>>());

        await _history.MarkAsAppliedAsync(mockMigration.Object);

        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.IsAny<MigrationRecord>(),
            It.Is<PartitionKey>(pk => pk.Equals(new PartitionKey("20240101_000001"))),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── MarkAsRolledBackAsync ────────────────────────────────────

    [Fact]
    public async Task MarkAsRolledBackAsync_WhenRecordNotFound_ShouldNotThrow()
    {
        await _history.InitializeAsync();

        _mockContainer
            .Setup(c => c.ReadItemAsync<MigrationRecord>(
                It.IsAny<string>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("Not Found", System.Net.HttpStatusCode.NotFound, 0, "", 0));

        var act = () => _history.MarkAsRolledBackAsync("nonexistent");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task MarkAsRolledBackAsync_WhenRecordExists_ShouldUpdateStatusToRolledBack()
    {
        await _history.InitializeAsync();

        var existingRecord = new MigrationRecord
        {
            Id = "20240101_000001",
            Name = "Test",
            AppliedAt = DateTime.UtcNow.AddDays(-1),
            Status = MigrationStatus.Applied
        };

        var mockResponse = new Mock<ItemResponse<MigrationRecord>>();
        mockResponse.Setup(r => r.Resource).Returns(existingRecord);

        _mockContainer
            .Setup(c => c.ReadItemAsync<MigrationRecord>(
                "20240101_000001",
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<MigrationRecord>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<MigrationRecord>>());

        await _history.MarkAsRolledBackAsync("20240101_000001");

        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.Is<MigrationRecord>(r => r.Status == MigrationStatus.RolledBack),
            It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task MarkAsRolledBackAsync_ShouldUpdateAppliedAt()
    {
        await _history.InitializeAsync();

        var oldTime = DateTime.UtcNow.AddDays(-10);
        var existingRecord = new MigrationRecord
        {
            Id = "test",
            Name = "Test",
            AppliedAt = oldTime,
            Status = MigrationStatus.Applied
        };

        var mockResponse = new Mock<ItemResponse<MigrationRecord>>();
        mockResponse.Setup(r => r.Resource).Returns(existingRecord);

        _mockContainer
            .Setup(c => c.ReadItemAsync<MigrationRecord>(
                "test", It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object);

        MigrationRecord? capturedRecord = null;
        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<MigrationRecord>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .Callback<MigrationRecord, PartitionKey?, ItemRequestOptions?, CancellationToken>(
                (r, _, _, _) => capturedRecord = r)
            .ReturnsAsync(Mock.Of<ItemResponse<MigrationRecord>>());

        var before = DateTime.UtcNow;
        await _history.MarkAsRolledBackAsync("test");
        var after = DateTime.UtcNow;

        capturedRecord.Should().NotBeNull();
        capturedRecord!.AppliedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    // ── Helper ───────────────────────────────────────────────────

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
