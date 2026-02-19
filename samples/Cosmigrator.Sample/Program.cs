using System.Reflection;

namespace Cosmigrator.Sample;

/// <summary>
/// Entry point for the sample Cosmigrator console application.
/// Delegates all infrastructure to <see cref="MigrationHost"/>.
/// </summary>
public static class Program
{
    public static async Task Main(string[] args)
    {
        await MigrationHost.RunAsync(args, Assembly.GetExecutingAssembly());
    }
}
