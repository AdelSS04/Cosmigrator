using System.Text.Json.Serialization;

namespace Cosmigrator.Models;

/// <summary>
/// Represents a migration record stored in the "__MigrationHistory" Cosmos DB container.
/// Tracks when a migration was applied and its current status.
/// </summary>
public class MigrationRecord
{
    /// <summary>
    /// The unique migration identifier (e.g. "20240101_000001").
    /// Also serves as the Cosmos DB document id and partition key.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name of the migration.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// UTC timestamp of when the migration was applied or rolled back.
    /// </summary>
    [JsonPropertyName("appliedAt")]
    public DateTime AppliedAt { get; set; }

    /// <summary>
    /// Current status of the migration: Applied or RolledBack.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MigrationStatus Status { get; set; }
}

/// <summary>
/// Possible statuses for a migration record.
/// </summary>
public enum MigrationStatus
{
    /// <summary>Migration has been successfully applied.</summary>
    Applied,

    /// <summary>Migration has been rolled back.</summary>
    RolledBack
}
