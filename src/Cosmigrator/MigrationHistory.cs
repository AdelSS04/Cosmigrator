using Cosmigrator.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Cosmigrator;

/// <summary>
/// Manages the "__MigrationHistory" container in Cosmos DB.
/// Tracks which migrations have been applied, rolled back, or are pending.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="MigrationHistory"/>.
/// </remarks>
/// <param name="database">The Cosmos DB database instance.</param>
/// <param name="logger">The logger instance.</param>
public class MigrationHistory(Database database, ILogger<MigrationHistory> logger)
{
    private const string ContainerName = "__MigrationHistory";

    private readonly Database _database = database;
    private readonly ILogger<MigrationHistory> _logger = logger;
    private Container? _container;

    /// <summary>
    /// Initializes the reference to the __MigrationHistory container.
    /// The container must already exist (e.g. created via Terraform).
    /// </summary>
    public Task InitializeAsync()
    {
        _container = _database.GetContainer(ContainerName);
        _logger.LogInformation("Migration history container '{ContainerName}' is ready", ContainerName);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns all migration records currently stored in the history container.
    /// </summary>
    public async Task<List<MigrationRecord>> GetAllRecordsAsync()
    {
        EnsureInitialized();

        var records = new List<MigrationRecord>();
        var query = new QueryDefinition("SELECT * FROM c ORDER BY c.id ASC");

        using var iterator = _container!.GetItemQueryIterator<MigrationRecord>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            records.AddRange(response);
        }

        return records;
    }

    /// <summary>
    /// Returns all migration records with status "Applied".
    /// </summary>
    public async Task<List<MigrationRecord>> GetAppliedMigrationsAsync()
    {
        var allRecords = await GetAllRecordsAsync();
        return [.. allRecords
            .Where(r => r.Status == MigrationStatus.Applied)
            .OrderBy(r => r.Id)];
    }

    /// <summary>
    /// Marks a migration as applied in the history container.
    /// </summary>
    /// <param name="migration">The migration that was applied.</param>
    public async Task MarkAsAppliedAsync(IMigration migration)
    {
        EnsureInitialized();

        var record = new MigrationRecord
        {
            Id = migration.Id,
            Name = migration.Name,
            AppliedAt = DateTime.UtcNow,
            Status = MigrationStatus.Applied
        };

        await _container!.UpsertItemAsync(record, new PartitionKey(record.Id));
    }

    /// <summary>
    /// Marks a migration as rolled back in the history container.
    /// </summary>
    /// <param name="migrationId">The ID of the migration to mark as rolled back.</param>
    public async Task MarkAsRolledBackAsync(string migrationId)
    {
        EnsureInitialized();

        try
        {
            var response = await _container!.ReadItemAsync<MigrationRecord>(
                migrationId, new PartitionKey(migrationId));

            var record = response.Resource;
            record.Status = MigrationStatus.RolledBack;
            record.AppliedAt = DateTime.UtcNow;

            await _container.UpsertItemAsync(record, new PartitionKey(record.Id));
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Migration record '{MigrationId}' not found in history", migrationId);
        }
    }

    private void EnsureInitialized()
    {
        if (_container is null)
            throw new InvalidOperationException(
                "MigrationHistory has not been initialized. Call InitializeAsync() first.");
    }
}
