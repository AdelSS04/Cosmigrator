using System.Text.Json;
using Cosmigrator.Models;
using FluentAssertions;

namespace Cosmigrator.Tests.Models;

public class MigrationRecordTests
{
    // ── Serialization ────────────────────────────────────────────

    [Fact]
    public void MigrationRecord_ShouldSerializeWithCorrectPropertyNames()
    {
        var record = new MigrationRecord
        {
            Id = "20240101_000001",
            Name = "TestMigration",
            AppliedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Status = MigrationStatus.Applied
        };

        var json = JsonSerializer.Serialize(record);
        var deserialized = JsonSerializer.Deserialize<MigrationRecord>(json);

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
        var record = new MigrationRecord
        {
            Id = "test",
            Name = "test",
            AppliedAt = DateTime.UtcNow,
            Status = MigrationStatus.Applied
        };

        var json = JsonSerializer.Serialize(record);

        json.Should().Contain("\"Applied\"");
        json.Should().NotContain("\"0\"");
    }

    [Fact]
    public void MigrationStatus_RolledBack_ShouldSerializeAsString()
    {
        var record = new MigrationRecord
        {
            Id = "test",
            Name = "test",
            AppliedAt = DateTime.UtcNow,
            Status = MigrationStatus.RolledBack
        };

        var json = JsonSerializer.Serialize(record);

        json.Should().Contain("\"RolledBack\"");
        json.Should().NotContain("\"1\"");
    }

    [Fact]
    public void MigrationStatus_ShouldDeserializeFromString()
    {
        var json = """
            {
                "id": "test",
                "name": "test",
                "appliedAt": "2024-01-01T00:00:00Z",
                "status": "RolledBack"
            }
            """;

        var record = JsonSerializer.Deserialize<MigrationRecord>(json);

        record.Should().NotBeNull();
        record!.Status.Should().Be(MigrationStatus.RolledBack);
    }

    [Fact]
    public void MigrationStatus_Applied_ShouldDeserializeFromString()
    {
        var json = """
            {
                "id": "test",
                "name": "test",
                "appliedAt": "2024-01-01T00:00:00Z",
                "status": "Applied"
            }
            """;

        var record = JsonSerializer.Deserialize<MigrationRecord>(json);

        record.Should().NotBeNull();
        record!.Status.Should().Be(MigrationStatus.Applied);
    }

    [Theory]
    [InlineData("20240101_000001")]
    [InlineData("20231231_235959")]
    [InlineData("test_migration")]
    public void MigrationRecord_ShouldAcceptValidIds(string id)
    {
        var record = new MigrationRecord
        {
            Id = id,
            Name = "Test",
            AppliedAt = DateTime.UtcNow,
            Status = MigrationStatus.Applied
        };

        record.Id.Should().Be(id);
    }

    // ── Default values ───────────────────────────────────────────

    [Fact]
    public void MigrationRecord_Defaults_IdShouldBeEmpty()
    {
        var record = new MigrationRecord();

        record.Id.Should().BeEmpty();
    }

    [Fact]
    public void MigrationRecord_Defaults_NameShouldBeEmpty()
    {
        var record = new MigrationRecord();

        record.Name.Should().BeEmpty();
    }

    [Fact]
    public void MigrationRecord_Defaults_AppliedAtShouldBeDefault()
    {
        var record = new MigrationRecord();

        record.AppliedAt.Should().Be(default(DateTime));
    }

    [Fact]
    public void MigrationRecord_Defaults_StatusShouldBeApplied()
    {
        var record = new MigrationRecord();

        record.Status.Should().Be(MigrationStatus.Applied); // 0 = Applied (first enum value)
    }

    // ── Roundtrip serialization ──────────────────────────────────

    [Fact]
    public void MigrationRecord_ShouldRoundtripThroughJsonSerialization()
    {
        var now = DateTime.UtcNow;
        var record = new MigrationRecord
        {
            Id = "20240315_000042",
            Name = "AddEmailColumn",
            AppliedAt = now,
            Status = MigrationStatus.Applied
        };

        var json = JsonSerializer.Serialize(record);
        var deserialized = JsonSerializer.Deserialize<MigrationRecord>(json);

        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be("20240315_000042");
        deserialized.Name.Should().Be("AddEmailColumn");
        deserialized.Status.Should().Be(MigrationStatus.Applied);
    }

    [Fact]
    public void MigrationRecord_RolledBack_ShouldRoundtrip()
    {
        var record = new MigrationRecord
        {
            Id = "20240101_000001",
            Name = "RolledBackMigration",
            AppliedAt = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc),
            Status = MigrationStatus.RolledBack
        };

        var json = JsonSerializer.Serialize(record);
        var deserialized = JsonSerializer.Deserialize<MigrationRecord>(json);

        deserialized!.Status.Should().Be(MigrationStatus.RolledBack);
        deserialized.Name.Should().Be("RolledBackMigration");
    }

    // ── Property setting ─────────────────────────────────────────

    [Fact]
    public void MigrationRecord_ShouldAllowSettingAllProperties()
    {
        var now = DateTime.UtcNow;
        var record = new MigrationRecord();

        record.Id = "my_id";
        record.Name = "my_name";
        record.AppliedAt = now;
        record.Status = MigrationStatus.RolledBack;

        record.Id.Should().Be("my_id");
        record.Name.Should().Be("my_name");
        record.AppliedAt.Should().Be(now);
        record.Status.Should().Be(MigrationStatus.RolledBack);
    }

    [Fact]
    public void MigrationRecord_StatusCanBeChangedFromAppliedToRolledBack()
    {
        var record = new MigrationRecord { Status = MigrationStatus.Applied };

        record.Status = MigrationStatus.RolledBack;

        record.Status.Should().Be(MigrationStatus.RolledBack);
    }

    // ── Enum values ──────────────────────────────────────────────

    [Fact]
    public void MigrationStatus_ShouldHaveTwoValues()
    {
        var values = Enum.GetValues<MigrationStatus>();

        values.Should().HaveCount(2);
        values.Should().Contain(MigrationStatus.Applied);
        values.Should().Contain(MigrationStatus.RolledBack);
    }

    [Fact]
    public void MigrationStatus_Applied_ShouldBeZero()
    {
        ((int)MigrationStatus.Applied).Should().Be(0);
    }

    [Fact]
    public void MigrationStatus_RolledBack_ShouldBeOne()
    {
        ((int)MigrationStatus.RolledBack).Should().Be(1);
    }

    // ── JSON property casing ─────────────────────────────────────

    [Fact]
    public void MigrationRecord_SerializedJson_ShouldUseCamelCase()
    {
        var record = new MigrationRecord
        {
            Id = "test",
            Name = "Test",
            AppliedAt = DateTime.UtcNow,
            Status = MigrationStatus.Applied
        };

        var json = JsonSerializer.Serialize(record);

        json.Should().Contain("\"id\":");
        json.Should().Contain("\"name\":");
        json.Should().Contain("\"appliedAt\":");
        json.Should().Contain("\"status\":");
        json.Should().NotContain("\"Id\":");
        json.Should().NotContain("\"Name\":");
        json.Should().NotContain("\"AppliedAt\":");
        json.Should().NotContain("\"Status\":");
    }

    [Fact]
    public void MigrationRecord_ShouldDeserializeFromCamelCaseJson()
    {
        var json = """{"id":"abc","name":"test","appliedAt":"2024-01-01T00:00:00Z","status":"Applied"}""";

        var record = JsonSerializer.Deserialize<MigrationRecord>(json);

        record.Should().NotBeNull();
        record!.Id.Should().Be("abc");
        record.Name.Should().Be("test");
        record.Status.Should().Be(MigrationStatus.Applied);
    }

    // ── Edge cases ───────────────────────────────────────────────

    [Fact]
    public void MigrationRecord_WithSpecialCharactersInId_ShouldSerialize()
    {
        var record = new MigrationRecord
        {
            Id = "20240101_special-chars_test.v2",
            Name = "Migration with special chars: <>&\"'",
            AppliedAt = DateTime.UtcNow,
            Status = MigrationStatus.Applied
        };

        var json = JsonSerializer.Serialize(record);
        var deserialized = JsonSerializer.Deserialize<MigrationRecord>(json);

        deserialized!.Id.Should().Be("20240101_special-chars_test.v2");
    }

    [Fact]
    public void MigrationRecord_WithEmptyStrings_ShouldSerialize()
    {
        var record = new MigrationRecord
        {
            Id = "",
            Name = "",
            AppliedAt = DateTime.MinValue,
            Status = MigrationStatus.Applied
        };

        var json = JsonSerializer.Serialize(record);
        var deserialized = JsonSerializer.Deserialize<MigrationRecord>(json);

        deserialized!.Id.Should().BeEmpty();
        deserialized.Name.Should().BeEmpty();
    }
}
