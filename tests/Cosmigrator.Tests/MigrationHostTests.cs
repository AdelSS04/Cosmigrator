using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Cosmigrator.Tests;

public class MigrationHostTests
{
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;

    public MigrationHostTests()
    {
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockLoggerFactory
            .Setup(f => f.CreateLogger(It.IsAny<string>()))
            .Returns(Mock.Of<ILogger>());
    }

    // ── Configuration validation ─────────────────────────────────

    [Fact]
    public async Task RunAsync_WithMissingConnectionString_ShouldThrow()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CosmosDb:DatabaseName"] = "TestDb"
            })
            .Build();

        var act = () => MigrationHost.RunAsync(config, _mockLoggerFactory.Object, [], typeof(MigrationHostTests).Assembly);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*ConnectionString*");
    }

    [Fact]
    public async Task RunAsync_WithMissingDatabaseName_ShouldThrow()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CosmosDb:ConnectionString"] = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=dGVzdA==;"
            })
            .Build();

        var act = () => MigrationHost.RunAsync(config, _mockLoggerFactory.Object, [], typeof(MigrationHostTests).Assembly);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*DatabaseName*");
    }

    [Fact]
    public async Task RunAsync_WithEmptyConfig_ShouldThrowForConnectionString()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var act = () => MigrationHost.RunAsync(config, _mockLoggerFactory.Object, [], typeof(MigrationHostTests).Assembly);

        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*ConnectionString*");
    }

    [Fact]
    public async Task RunAsync_WithBothConfigValuesMissing_ShouldThrowForConnectionStringFirst()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var act = () => MigrationHost.RunAsync(config, _mockLoggerFactory.Object, ["migrate"], typeof(MigrationHostTests).Assembly);

        // ConnectionString is checked first
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*ConnectionString*");
    }

    [Fact]
    public void RunAsync_WithNullLoggerFactory_ShouldThrow()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CosmosDb:ConnectionString"] = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=dGVzdA==;",
                ["CosmosDb:DatabaseName"] = "TestDb"
            })
            .Build();

        var act = () => MigrationHost.RunAsync(config, null!, ["migrate"], typeof(MigrationHostTests).Assembly);

        act.Should().ThrowAsync<Exception>();
    }
}
