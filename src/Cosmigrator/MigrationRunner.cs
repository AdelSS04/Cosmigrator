using System.Reflection;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace Cosmigrator;

/// <summary>
/// Orchestrates discovering, running, rolling back, and reporting
/// on Cosmos DB migrations. This is the central engine of the migrator.
/// </summary>
public class MigrationRunner
{
    private readonly CosmosClient _client;
    private readonly Database _database;
    private readonly MigrationHistory _history;
    private readonly List<IMigration> _migrations;
    private readonly ILogger<MigrationRunner> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="MigrationRunner"/>.
    /// </summary>
    /// <param name="client">The Cosmos DB client.</param>
    /// <param name="databaseName">The target database name.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="migrationAssemblies">
    /// Assemblies to scan for <see cref="IMigration"/> implementations.
    /// Defaults to the entry assembly if none specified.
    /// </param>
    public MigrationRunner(
        CosmosClient client,
        string databaseName,
        ILoggerFactory loggerFactory,
        params Assembly[] migrationAssemblies)
    {
        _client = client;
        _database = _client.GetDatabase(databaseName);
        _logger = loggerFactory.CreateLogger<MigrationRunner>();
        _history = new MigrationHistory(_database, loggerFactory.CreateLogger<MigrationHistory>());
        _migrations = MigrationDiscovery.DiscoverAll(migrationAssemblies);
    }

    /// <summary>
    /// Initializes the migration history container reference. Must be called before
    /// any other operations.
    /// </summary>
    public async Task InitializeAsync() => await _history.InitializeAsync();

    /// <summary>
    /// Discovers and runs all pending migrations in chronological order.
    /// Exits with code 0 on success, code 1 on failure.
    /// </summary>
    public async Task RunPendingMigrationsAsync()
    {
        _logger.LogInformation("Running pending migrations");

        var applied = await _history.GetAppliedMigrationsAsync();
        var appliedIds = new HashSet<string>(applied.Select(a => a.Id));

        var pending = _migrations
            .Where(m => !appliedIds.Contains(m.Id))
            .OrderBy(m => m.Id)
            .ToList();

        if (pending.Count == 0)
        {
            _logger.LogInformation("No pending migrations found. Database is up to date");
            Environment.Exit(0);
            return;
        }

        _logger.LogInformation("Found {Count} pending migration(s)", pending.Count);

        foreach (var migration in pending)
        {
            _logger.LogInformation("Applying: [{Id}] {Name} -> container '{Container}'",
                migration.Id, migration.Name, migration.ContainerName);

            try
            {
                var container = _database.GetContainer(migration.ContainerName);
                await migration.UpAsync(container, _client);

                await _history.MarkAsAppliedAsync(migration);
                _logger.LogInformation("Applied: [{Id}] {Name}", migration.Id, migration.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed: [{Id}] {Name}", migration.Id, migration.Name);
                _logger.LogError("Migration run aborted. Fix the issue and re-run");
                Environment.Exit(1);
                return;
            }
        }

        _logger.LogInformation("All {Count} migration(s) applied successfully", pending.Count);
        Environment.Exit(0);
    }

    /// <summary>
    /// Rolls back the last <paramref name="steps"/> applied migrations in reverse order.
    /// </summary>
    /// <param name="steps">Number of migrations to roll back.</param>
    public async Task RollbackAsync(int steps = 1)
    {
        _logger.LogInformation("Rolling back last {Steps} migration(s)", steps);

        var applied = await _history.GetAppliedMigrationsAsync();

        if (applied.Count == 0)
        {
            _logger.LogWarning("No applied migrations to roll back");
            Environment.Exit(0);
            return;
        }

        var toRollback = applied
            .OrderByDescending(a => a.Id)
            .Take(steps)
            .ToList();

        _logger.LogInformation("Rolling back {Count} migration(s)...", toRollback.Count);

        foreach (var record in toRollback)
        {
            var migration = _migrations.FirstOrDefault(m => m.Id == record.Id);
            if (migration is null)
            {
                _logger.LogWarning(
                    "Migration class for [{Id}] {Name} not found in assembly. Skipping rollback",
                    record.Id, record.Name);
                continue;
            }

            _logger.LogInformation("Rolling back: [{Id}] {Name}", migration.Id, migration.Name);

            try
            {
                var container = _database.GetContainer(migration.ContainerName);
                await migration.DownAsync(container, _client);

                await _history.MarkAsRolledBackAsync(migration.Id);
                _logger.LogInformation("Rolled back: [{Id}] {Name}", migration.Id, migration.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Rollback failed: [{Id}] {Name}", migration.Id, migration.Name);
                _logger.LogError("Rollback aborted. Fix the issue and re-run");
                Environment.Exit(1);
                return;
            }
        }

        _logger.LogInformation("Rolled back {Count} migration(s) successfully", toRollback.Count);
        Environment.Exit(0);
    }

    /// <summary>
    /// Prints the status of all discovered migrations (Applied / Pending / RolledBack).
    /// </summary>
    public async Task PrintStatusAsync()
    {
        _logger.LogInformation("Migration Status");

        var records = await _history.GetAllRecordsAsync();
        var recordMap = records.ToDictionary(r => r.Id);

        foreach (var migration in _migrations)
        {
            if (recordMap.TryGetValue(migration.Id, out var record))
            {
                _logger.LogInformation("[{Id}] {Name} - {Status} at {AppliedAt:yyyy-MM-dd HH:mm:ss} UTC",
                    migration.Id, migration.Name, record.Status, record.AppliedAt);
            }
            else
            {
                _logger.LogInformation("[{Id}] {Name} - Pending", migration.Id, migration.Name);
            }
        }
    }

    /// <summary>
    /// Lists all discovered migration classes from the assembly.
    /// </summary>
    public void PrintDiscoveredMigrations()
    {
        _logger.LogInformation("Discovered Migrations");

        if (_migrations.Count == 0)
        {
            _logger.LogWarning("No migration classes found in assembly");
            return;
        }

        foreach (var migration in _migrations)
        {
            _logger.LogInformation("[{Id}] {Name} -> {Container}",
                migration.Id, migration.Name, migration.ContainerName);
        }

        _logger.LogInformation("Total: {Count} migration(s) discovered", _migrations.Count);
    }
}
