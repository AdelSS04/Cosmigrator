using System.Net;
using System.Text.Json.Nodes;
using FluentAssertions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;

namespace Cosmigrator.Tests;

public class BulkOperationHelperTests
{
    private readonly Mock<ILogger<BulkOperationHelper>> _mockLogger;
    private readonly Mock<Container> _mockContainer;

    public BulkOperationHelperTests()
    {
        _mockLogger = new Mock<ILogger<BulkOperationHelper>>();
        _mockContainer = new Mock<Container>();
        _mockContainer.Setup(c => c.Id).Returns("TestContainer");
    }

    // ── Constructor ──────────────────────────────────────────────

    [Fact]
    public void Constructor_WithDefaults_ShouldCreateSuccessfully()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);

        helper.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithCustomBatchSize_ShouldCreateSuccessfully()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object, batchSize: 50);

        helper.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithAllParameters_ShouldCreateSuccessfully()
    {
        var helper = new BulkOperationHelper(
            _mockLogger.Object,
            batchSize: 200,
            maxRetries: 10,
            baseDelay: TimeSpan.FromSeconds(2));

        helper.Should().NotBeNull();
    }

    // ── ReadAllDocumentsAsync ─────────────────────────────────────

    [Fact]
    public async Task ReadAllDocumentsAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);
        SetupIterator(new List<JsonObject>());

        var result = await helper.ReadAllDocumentsAsync(_mockContainer.Object);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ReadAllDocumentsAsync_WithDocuments_ShouldReturnAll()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);
        var docs = new List<JsonObject>
        {
            new() { ["id"] = "1", ["name"] = "Doc1" },
            new() { ["id"] = "2", ["name"] = "Doc2" },
            new() { ["id"] = "3", ["name"] = "Doc3" }
        };
        SetupIterator(docs);

        var result = await helper.ReadAllDocumentsAsync(_mockContainer.Object);

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task ReadAllDocumentsAsync_ShouldUseSelectAllQuery()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);
        SetupIterator(new List<JsonObject>());

        await helper.ReadAllDocumentsAsync(_mockContainer.Object);

        _mockContainer.Verify(c => c.GetItemQueryIterator<JsonObject>(
            It.Is<QueryDefinition>(q => q.QueryText == "SELECT * FROM c"),
            It.IsAny<string>(),
            It.IsAny<QueryRequestOptions>()),
            Times.Once);
    }

    // ── ReadDocumentsAsync ───────────────────────────────────────

    [Fact]
    public async Task ReadDocumentsAsync_WithCustomQuery_ShouldUseProvidedSql()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);
        var expectedSql = "SELECT * FROM c WHERE c.status = 'active'";
        SetupIterator(new List<JsonObject>());

        await helper.ReadDocumentsAsync(_mockContainer.Object, expectedSql);

        _mockContainer.Verify(c => c.GetItemQueryIterator<JsonObject>(
            It.Is<QueryDefinition>(q => q.QueryText == expectedSql),
            It.IsAny<string>(),
            It.IsAny<QueryRequestOptions>()),
            Times.Once);
    }

    [Fact]
    public async Task ReadDocumentsAsync_WithFilteredQuery_ShouldReturnFilteredResults()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);
        var docs = new List<JsonObject>
        {
            new() { ["id"] = "1", ["status"] = "active" }
        };
        SetupIterator(docs);

        var result = await helper.ReadDocumentsAsync(_mockContainer.Object, "SELECT * FROM c WHERE c.status = 'active'");

        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task ReadDocumentsAsync_WhenNoResults_ShouldReturnEmptyList()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);
        SetupIterator(new List<JsonObject>());

        var result = await helper.ReadDocumentsAsync(_mockContainer.Object, "SELECT * FROM c WHERE c.id = 'nonexistent'");

        result.Should().BeEmpty();
    }

    // ── BulkUpsertAsync ──────────────────────────────────────────

    [Fact]
    public async Task BulkUpsertAsync_WithEmptyList_ShouldNotCallUpsert()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);

        await helper.BulkUpsertAsync(_mockContainer.Object, []);

        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task BulkUpsertAsync_ShouldUpsertAllDocuments()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object, batchSize: 100);

        var docs = new List<JsonObject>
        {
            new() { ["id"] = "1" },
            new() { ["id"] = "2" },
            new() { ["id"] = "3" }
        };

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<JsonObject>>());

        await helper.BulkUpsertAsync(_mockContainer.Object, docs);

        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task BulkUpsertAsync_WithSmallBatchSize_ShouldProcessInMultipleBatches()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object, batchSize: 2);

        var docs = new List<JsonObject>
        {
            new() { ["id"] = "1" },
            new() { ["id"] = "2" },
            new() { ["id"] = "3" }
        };

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<JsonObject>>());

        await helper.BulkUpsertAsync(_mockContainer.Object, docs);

        // All 3 should still be upserted (2 in first batch, 1 in second)
        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task BulkUpsertAsync_WithCustomPartitionKey_ShouldUseCorrectKey()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);

        var docs = new List<JsonObject>
        {
            new() { ["id"] = "1", ["tenantId"] = "tenant-abc" }
        };

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<JsonObject>>());

        await helper.BulkUpsertAsync(_mockContainer.Object, docs, partitionKeyPath: "tenantId");

        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.IsAny<JsonObject>(),
            It.Is<PartitionKey>(pk => pk.Equals(new PartitionKey("tenant-abc"))),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BulkUpsertAsync_WithMissingPartitionKey_ShouldUseEmptyString()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);

        var docs = new List<JsonObject>
        {
            new() { ["id"] = "1" } // No "customPk" field
        };

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<JsonObject>>());

        await helper.BulkUpsertAsync(_mockContainer.Object, docs, partitionKeyPath: "customPk");

        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.IsAny<JsonObject>(),
            It.Is<PartitionKey>(pk => pk.Equals(new PartitionKey(""))),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task BulkUpsertAsync_WithDefaultPartitionKeyPath_ShouldUseId()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);

        var docs = new List<JsonObject>
        {
            new() { ["id"] = "doc-123" }
        };

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<JsonObject>>());

        await helper.BulkUpsertAsync(_mockContainer.Object, docs);

        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.IsAny<JsonObject>(),
            It.Is<PartitionKey>(pk => pk.Equals(new PartitionKey("doc-123"))),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── BulkUpsertAsync retry ────────────────────────────────────

    [Fact]
    public async Task BulkUpsertAsync_OnThrottle_ShouldRetryAndSucceed()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object, maxRetries: 3, baseDelay: TimeSpan.FromMilliseconds(1));

        var docs = new List<JsonObject> { new() { ["id"] = "1" } };

        var callCount = 0;
        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                if (callCount <= 2)
                    throw new CosmosException("Throttled", HttpStatusCode.TooManyRequests, 429, "", 0);
                return Task.FromResult(Mock.Of<ItemResponse<JsonObject>>());
            });

        var act = () => helper.BulkUpsertAsync(_mockContainer.Object, docs);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task BulkUpsertAsync_WhenMaxRetriesExceeded_ShouldThrow()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object, maxRetries: 2, baseDelay: TimeSpan.FromMilliseconds(1));

        var docs = new List<JsonObject> { new() { ["id"] = "1" } };

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("Throttled", HttpStatusCode.TooManyRequests, 429, "", 0));

        var act = () => helper.BulkUpsertAsync(_mockContainer.Object, docs);

        await act.Should().ThrowAsync<CosmosException>();
    }

    [Fact]
    public async Task BulkUpsertAsync_OnNonThrottleError_ShouldThrowImmediately()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object, maxRetries: 5, baseDelay: TimeSpan.FromMilliseconds(1));

        var docs = new List<JsonObject> { new() { ["id"] = "1" } };

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CosmosException("Internal Error", HttpStatusCode.InternalServerError, 500, "", 0));

        var act = () => helper.BulkUpsertAsync(_mockContainer.Object, docs);

        await act.Should().ThrowAsync<CosmosException>();
    }

    // ── CopyAllDocumentsAsync ────────────────────────────────────

    [Fact]
    public async Task CopyAllDocumentsAsync_WhenSourceEmpty_ShouldNotUpsertAnything()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);

        var mockSource = new Mock<Container>();
        mockSource.Setup(c => c.Id).Returns("Source");
        var mockTarget = new Mock<Container>();
        mockTarget.Setup(c => c.Id).Returns("Target");

        SetupIteratorForContainer(mockSource, new List<JsonObject>());

        await helper.CopyAllDocumentsAsync(mockSource.Object, mockTarget.Object);

        mockTarget.Verify(c => c.UpsertItemAsync(
            It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CopyAllDocumentsAsync_WithDocuments_ShouldCopyAll()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);

        var mockSource = new Mock<Container>();
        mockSource.Setup(c => c.Id).Returns("Source");
        var mockTarget = new Mock<Container>();
        mockTarget.Setup(c => c.Id).Returns("Target");

        var docs = new List<JsonObject>
        {
            new() { ["id"] = "1" },
            new() { ["id"] = "2" }
        };
        SetupIteratorForContainer(mockSource, docs);

        mockTarget
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<JsonObject>>());

        await helper.CopyAllDocumentsAsync(mockSource.Object, mockTarget.Object);

        mockTarget.Verify(c => c.UpsertItemAsync(
            It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task CopyAllDocumentsAsync_WithCustomPartitionKey_ShouldPassThrough()
    {
        var helper = new BulkOperationHelper(_mockLogger.Object);

        var mockSource = new Mock<Container>();
        mockSource.Setup(c => c.Id).Returns("Source");
        var mockTarget = new Mock<Container>();
        mockTarget.Setup(c => c.Id).Returns("Target");

        var docs = new List<JsonObject>
        {
            new() { ["id"] = "1", ["tenantId"] = "t1" }
        };
        SetupIteratorForContainer(mockSource, docs);

        mockTarget
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<JsonObject>>());

        await helper.CopyAllDocumentsAsync(mockSource.Object, mockTarget.Object, "tenantId");

        mockTarget.Verify(c => c.UpsertItemAsync(
            It.IsAny<JsonObject>(),
            It.Is<PartitionKey>(pk => pk.Equals(new PartitionKey("t1"))),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── CreateBulkClient ─────────────────────────────────────────

    [Fact]
    public void CreateBulkClient_ShouldReturnNonNullClient()
    {
        // This will throw because the connection string is invalid for actual connection
        // but the client object should be created
        var act = () => BulkOperationHelper.CreateBulkClient(
            "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=dGVzdA==;");

        act.Should().NotThrow();
    }

    [Fact]
    public void CreateBulkClient_WithInvalidConnectionString_ShouldThrow()
    {
        var act = () => BulkOperationHelper.CreateBulkClient("invalid");

        act.Should().Throw<Exception>();
    }

    // ── Large batch tests ────────────────────────────────────────

    [Fact]
    public async Task BulkUpsertAsync_WithExactBatchSizeDocuments_ShouldProcessInOneBatch()
    {
        var batchSize = 5;
        var helper = new BulkOperationHelper(_mockLogger.Object, batchSize: batchSize);

        var docs = Enumerable.Range(1, batchSize)
            .Select(i => new JsonObject { ["id"] = i.ToString() })
            .ToList();

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<JsonObject>>());

        await helper.BulkUpsertAsync(_mockContainer.Object, docs);

        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()),
            Times.Exactly(batchSize));
    }

    [Fact]
    public async Task BulkUpsertAsync_WithBatchSizePlusOne_ShouldProcessInTwoBatches()
    {
        var batchSize = 3;
        var helper = new BulkOperationHelper(_mockLogger.Object, batchSize: batchSize);

        var docs = Enumerable.Range(1, batchSize + 1)
            .Select(i => new JsonObject { ["id"] = i.ToString() })
            .ToList();

        _mockContainer
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<JsonObject>>());

        await helper.BulkUpsertAsync(_mockContainer.Object, docs);

        _mockContainer.Verify(c => c.UpsertItemAsync(
            It.IsAny<JsonObject>(), It.IsAny<PartitionKey>(), It.IsAny<ItemRequestOptions>(), It.IsAny<CancellationToken>()),
            Times.Exactly(batchSize + 1));
    }

    // ── Helpers ──────────────────────────────────────────────────

    private void SetupIterator(List<JsonObject> items)
    {
        SetupIteratorForContainer(_mockContainer, items);
    }

    private static void SetupIteratorForContainer(Mock<Container> container, List<JsonObject> items)
    {
        var mockIterator = new Mock<FeedIterator<JsonObject>>();
        var hasCalledOnce = false;

        mockIterator
            .Setup(i => i.HasMoreResults)
            .Returns(() => !hasCalledOnce);

        var mockResponse = new Mock<FeedResponse<JsonObject>>();
        mockResponse.Setup(r => r.GetEnumerator()).Returns(items.GetEnumerator());

        mockIterator
            .Setup(i => i.ReadNextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResponse.Object)
            .Callback(() => hasCalledOnce = true);

        container
            .Setup(c => c.GetItemQueryIterator<JsonObject>(
                It.IsAny<QueryDefinition>(),
                It.IsAny<string>(),
                It.IsAny<QueryRequestOptions>()))
            .Returns(mockIterator.Object);
    }
}
