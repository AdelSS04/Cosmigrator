using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Cosmigrator;

/// <summary>
/// Provides helper methods for performing bulk document operations in Cosmos DB.
/// Uses <c>AllowBulkExecution = true</c> for optimal throughput and includes
/// automatic retry with exponential backoff for 429 (TooManyRequests) responses.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="BulkOperationHelper"/>.
/// </remarks>
/// <param name="logger">The logger instance.</param>
/// <param name="batchSize">Number of documents to process per batch (default: 100).</param>
/// <param name="maxRetries">Maximum retry attempts for throttled requests (default: 5).</param>
/// <param name="baseDelay">Base delay for exponential backoff (default: 1 second).</param>
public class BulkOperationHelper(ILogger<BulkOperationHelper> logger, int batchSize = 100, int maxRetries = 5, TimeSpan? baseDelay = null)
{
    private readonly int _batchSize = batchSize;
    private readonly int _maxRetries = maxRetries;
    private readonly TimeSpan _baseDelay = baseDelay ?? TimeSpan.FromSeconds(1);
    private readonly ILogger<BulkOperationHelper> _logger = logger;

    /// <summary>
    /// Reads all documents from a container using a FeedIterator.
    /// </summary>
    /// <param name="container">The source container.</param>
    /// <returns>A list of all documents as <see cref="JsonObject"/>.</returns>
    public async Task<List<JsonObject>> ReadAllDocumentsAsync(Container container)
    {
        var documents = new List<JsonObject>();
        var query = new QueryDefinition("SELECT * FROM c");

        using var iterator = container.GetItemQueryIterator<JsonObject>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            documents.AddRange(response);
        }

        _logger.LogInformation("Read {Count} document(s) from '{ContainerId}'", documents.Count, container.Id);
        return documents;
    }

    /// <summary>
    /// Upserts a collection of documents into a container in configurable batches,
    /// reporting progress and handling 429 throttling with exponential backoff.
    /// </summary>
    /// <param name="container">The target container.</param>
    /// <param name="documents">The documents to upsert.</param>
    /// <param name="partitionKeyPath">
    /// The partition key property name (without leading '/'), e.g. "id".
    /// </param>
    public async Task BulkUpsertAsync(Container container, List<JsonObject> documents, string partitionKeyPath = "id")
    {
        var total = documents.Count;
        var processed = 0;

        for (var i = 0; i < total; i += _batchSize)
        {
            var batch = documents.Skip(i).Take(_batchSize).ToList();
            var tasks = new List<Task>();

            foreach (var doc in batch)
            {
                var pkValue = doc[partitionKeyPath]?.ToString() ?? string.Empty;
                tasks.Add(UpsertWithRetryAsync(container, doc, new PartitionKey(pkValue)));
            }

            await Task.WhenAll(tasks);

            processed += batch.Count;
            _logger.LogInformation("Processed {Processed}/{Total} documents...", processed, total);
        }

        _logger.LogInformation("Bulk upsert complete. {Total} document(s) processed", total);
    }

    /// <summary>
    /// Copies all documents from one container to another using bulk operations.
    /// </summary>
    /// <param name="source">The source container.</param>
    /// <param name="target">The target container.</param>
    /// <param name="partitionKeyPath">The partition key property name (without leading '/').</param>
    public async Task CopyAllDocumentsAsync(Container source, Container target, string partitionKeyPath = "id")
    {
        _logger.LogInformation("Copying documents from '{SourceId}' to '{TargetId}'...", source.Id, target.Id);

        var documents = await ReadAllDocumentsAsync(source);

        if (documents.Count == 0)
        {
            _logger.LogWarning("No documents to copy");
            return;
        }

        await BulkUpsertAsync(target, documents, partitionKeyPath);
    }

    /// <summary>
    /// Creates a Cosmos DB client configured for bulk execution.
    /// </summary>
    /// <param name="connectionString">The Cosmos DB connection string.</param>
    /// <returns>A <see cref="CosmosClient"/> with bulk execution enabled.</returns>
    public static CosmosClient CreateBulkClient(string connectionString)
    {
        return new CosmosClient(connectionString, new CosmosClientOptions
        {
            UseSystemTextJsonSerializerWithOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            },
            AllowBulkExecution = true,
            MaxRetryAttemptsOnRateLimitedRequests = 10,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(60)
        });
    }

    /// <summary>
    /// Upserts a single document with retry logic for 429 (TooManyRequests).
    /// Uses exponential backoff between retries.
    /// </summary>
    private async Task UpsertWithRetryAsync(Container container, JsonObject document, PartitionKey partitionKey)
    {
        for (var attempt = 0; attempt <= _maxRetries; attempt++)
        {
            try
            {
                await container.UpsertItemAsync(document, partitionKey);
                return;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                if (attempt == _maxRetries)
                {
                    _logger.LogError("Max retries ({MaxRetries}) exceeded for document '{DocumentId}'",
                        _maxRetries, document["id"]);
                    throw;
                }

                var delay = ex.RetryAfter ?? TimeSpan.FromMilliseconds(
                    _baseDelay.TotalMilliseconds * Math.Pow(2, attempt));

                _logger.LogWarning("Throttled (429). Retrying in {Delay:F1}s (attempt {Attempt}/{MaxRetries})...",
                    delay.TotalSeconds, attempt + 1, _maxRetries);

                await Task.Delay(delay);
            }
        }
    }
}
