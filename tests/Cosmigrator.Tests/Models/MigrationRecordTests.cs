using System.Text.Json;
using Cosmigrator.Models;
using FluentAssertions;

namespace Cosmigrator.Tests.Models;

public class MigrationRecordTests
{
    [Fact]
    public void MigrationRecord_ShouldSerializeWithCorrectPropertyNames()
    {
        // Arrange
        var record = new MigrationRecord
        {
            Id = "20240101_000001",
            Name = "TestMigration",
            AppliedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Status = MigrationStatus.Applied
        };

        // Act
        var json = JsonSerializer.Serialize(record);
        var deserialized = JsonSerializer.Deserialize<MigrationRecord>(json);

        // Assert
        json.Should().Contain("\"id\":");
        json.Should().Contain("\"name\":");
        json.Should().Contain("\"appliedAt\":");
        json.Should().Contain("\"status\":");

        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(record.Id);
        deserialized.Name.Should().Be(record.Name);
        deserialized.AppliedAt.Should().Be(record.AppliedAt);
        deserialized.Status.Should().Be(record.Status);
    }

    [Fact]
    public void MigrationStatus_ShouldSerializeAsString()
    {
        // Arrange
        var record = new MigrationRecord
        {
            Id = "test",
            Name = "test",
            AppliedAt = DateTime.UtcNow,
            Status = MigrationStatus.Applied
        };

        // Act
        var json = JsonSerializer.Serialize(record);

        // Assert
        json.Should().Contain("\"Applied\"");
        json.Should().NotContain("\"0\"");
    }

    [Fact]
    public void MigrationStatus_ShouldDeserializeFromString()
    {
        // Arrange
        var json = """
            {
                "id": "test",
                "name": "test",
                "appliedAt": "2024-01-01T00:00:00Z",
                "status": "RolledBack"
            }
            """;

        // Act
        var record = JsonSerializer.Deserialize<MigrationRecord>(json);

        // Assert
        record.Should().NotBeNull();
        record!.Status.Should().Be(MigrationStatus.RolledBack);
    }

    [Theory]
    [InlineData("20240101_000001")]
    [InlineData("20231231_235959")]
    [InlineData("test_migration")]
    public void MigrationRecord_ShouldAcceptValidIds(string id)
    {
        // Arrange & Act
        var record = new MigrationRecord
        {
            Id = id,
            Name = "Test",
            AppliedAt = DateTime.UtcNow,
            Status = MigrationStatus.Applied
        };

        // Assert
        record.Id.Should().Be(id);
    }
}
