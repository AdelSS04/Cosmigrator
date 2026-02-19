using System.Reflection;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Cosmigrator;

/// <summary>
/// Static entry point for running Cosmos DB migrations.
/// Handles all common infrastructure: config loading, Cosmos client creation,
/// CLI command parsing, and delegation to <see cref="MigrationRunner"/>.
/// </summary>
public static class MigrationHost
{
    /// <summary>
    /// Simplified entry point that handles complete application bootstrap including
    /// Host building, Serilog configuration, and migration execution.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <param name="migrationAssembly">The assembly containing <see cref="IMigration"/> implementations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RunAsync(string[] args, Assembly migrationAssembly)
    {
        // Bootstrap Serilog with console output before Host is built
        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) =>
            {
                config
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args);
            })
            .UseSerilog((context, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            })
            .Build();

        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        var configuration = host.Services.GetRequiredService<IConfiguration>();

        try
        {
            await RunAsync(configuration, loggerFactory, args, migrationAssembly);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application start-up failed");
            Environment.Exit(1);
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    /// <summary>
    /// Runs the migration pipeline with the provided configuration and logging.
    /// Parses CLI arguments and executes the appropriate command (migrate, rollback, status, list).
    /// </summary>
    /// <param name="configuration">The application configuration containing CosmosDb settings.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="args">Command line arguments.</param>
    /// <param name="migrationAssembly">The assembly containing <see cref="IMigration"/> implementations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="Exception">Thrown when required configuration is missing.</exception>
    public static async Task RunAsync(
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        string[] args,
        Assembly migrationAssembly)
    {
        var logger = loggerFactory.CreateLogger("Cosmigrator");

        var connectionString = configuration["CosmosDb:ConnectionString"]
            ?? throw new Exception("Missing CosmosDb:ConnectionString configuration");

        var databaseName = configuration["CosmosDb:DatabaseName"]
            ?? throw new Exception("Missing CosmosDb:DatabaseName configuration");

        logger.LogInformation("Initializing Cosmos DB connection to database '{DatabaseName}'", databaseName);

        using var client = new CosmosClient(connectionString, new CosmosClientOptions
        {
            AllowBulkExecution = true,
            MaxRetryAttemptsOnRateLimitedRequests = 10,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(60)
        });

        var migrationRunner = new MigrationRunner(
            client,
            databaseName,
            loggerFactory,
            migrationAssembly);

        await migrationRunner.InitializeAsync();

        // ── Parse CLI command ──────────────────────────────────────
        var command = args.Length > 0 ? args[0].ToLower() : "migrate";

        switch (command)
        {
            case "migrate":
                await migrationRunner.RunPendingMigrationsAsync();
                break;

            case "rollback":
                var steps = args.Contains("--steps")
                    ? int.Parse(args[Array.IndexOf(args, "--steps") + 1])
                    : 1;
                await migrationRunner.RollbackAsync(steps);
                break;

            case "status":
                await migrationRunner.PrintStatusAsync();
                break;

            case "list":
                migrationRunner.PrintDiscoveredMigrations();
                break;

            default:
                logger.LogError("Unknown command: {Command}", command);
                logger.LogInformation("Usage: dotnet run [migrate|rollback|status|list] [--steps N]");
                Environment.Exit(1);
                break;
        }
    }
}
