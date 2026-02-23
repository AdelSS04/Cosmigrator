---
title: BulkOperationHelper
sidebar_position: 5
---

# BulkOperationHelper

Provides helper methods for performing bulk document operations in Cosmos DB. Includes automatic retry with exponential backoff for 429 (TooManyRequests) responses.

```csharp
namespace Cosmigrator;

public class BulkOperationHelper(
    ILogger<BulkOperationHelper> logger,
    int batchSize = 100,
    int maxRetries = 5,
    TimeSpan? baseDelay = null)
{
    public async Task<List<JsonObject>> ReadAllDocumentsAsync(Container container);
    public async Task<List<JsonObject>> ReadDocumentsAsync(Container container, string sql);
    public async Task BulkUpsertAsync(Container container, List<JsonObject> documents, string partitionKeyPath = "id");
    public async Task CopyAllDocumentsAsync(Container source, Container target, string partitionKeyPath = "id");
    public static CosmosClient CreateBulkClient(string connectionString);
}
```

## Constructor

Uses a C# 12 primary constructor.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `logger` | `ILogger<BulkOperationHelper>` | — | Logger instance |
| `batchSize` | `int` | `100` | Number of documents to process per batch |
| `maxRetries` | `int` | `5` | Maximum retry attempts for throttled requests |
| `baseDelay` | `TimeSpan?` | `1 second` | Base delay for exponential backoff |

```csharp
using var loggerFactory = LoggerFactory.Create(b => b.AddConsole());
var helper = new BulkOperationHelper(
    loggerFactory.CreateLogger<BulkOperationHelper>(),
    batchSize: 200,
    maxRetries: 10,
    baseDelay: TimeSpan.FromSeconds(2));
```

## Methods

| Method | Returns | Description |
|--------|---------|-------------|
| `ReadAllDocumentsAsync(Container)` | `Task<List<JsonObject>>` | Reads all documents from a container using `SELECT * FROM c` |
| `ReadDocumentsAsync(Container, string)` | `Task<List<JsonObject>>` | Reads documents using a custom SQL query |
| `BulkUpsertAsync(Container, List<JsonObject>, string)` | `Task` | Upserts documents in batches with retry logic |
| `CopyAllDocumentsAsync(Container, Container, string)` | `Task` | Reads all from source, bulk upserts to target |
| `CreateBulkClient(string)` | `CosmosClient` | Static — creates a client configured for bulk execution |

## ReadAllDocumentsAsync

```csharp
var docs = await helper.ReadAllDocumentsAsync(container);
```

Delegates to `ReadDocumentsAsync(container, "SELECT * FROM c")`. Use `ReadDocumentsAsync` directly with a filtered query for better performance.

## ReadDocumentsAsync

```csharp
var docs = await helper.ReadDocumentsAsync(
    container,
    "SELECT * FROM c WHERE NOT IS_DEFINED(c.email)");
```

| Parameter | Type | Description |
|-----------|------|-------------|
| `container` | `Container` | Source container |
| `sql` | `string` | Cosmos DB SQL query string |

Uses a `FeedIterator` to page through all results. Logs the document count and query string.

## BulkUpsertAsync

```csharp
await helper.BulkUpsertAsync(container, documents, "customerId");
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `container` | `Container` | — | Target container |
| `documents` | `List<JsonObject>` | — | Documents to upsert |
| `partitionKeyPath` | `string` | `"id"` | Partition key property name (without `/`) |

Processing:
1. Splits documents into batches of `batchSize`
2. Each batch fires all upserts concurrently via `Task.WhenAll`
3. Each individual upsert uses `UpsertWithRetryAsync` — on 429, waits using either the server's `RetryAfter` header or exponential backoff (`baseDelay * 2^attempt`)
4. Logs progress after each batch

## CopyAllDocumentsAsync

```csharp
await helper.CopyAllDocumentsAsync(sourceContainer, targetContainer, "customerId");
```

Reads all documents from source with `ReadAllDocumentsAsync`, then bulk upserts to target. Used in container swap migrations (e.g., [unique key policy changes](../migrations/unique-key-policy)).

## CreateBulkClient

```csharp
var client = BulkOperationHelper.CreateBulkClient(connectionString);
```

Returns a `CosmosClient` configured with:
- `System.Text.Json` serialization with camelCase naming
- `AllowBulkExecution = true`
- 10 retry attempts, 60s max wait on rate limiting

## Retry behavior

When a 429 (TooManyRequests) response occurs:

1. Uses `RetryAfter` from the response if available
2. Falls back to exponential backoff: `baseDelay * 2^attempt`
3. After `maxRetries` attempts, throws the `CosmosException`

```
Attempt 1: wait 1s (or server RetryAfter)
Attempt 2: wait 2s
Attempt 3: wait 4s
Attempt 4: wait 8s
Attempt 5: wait 16s → then throw
```
