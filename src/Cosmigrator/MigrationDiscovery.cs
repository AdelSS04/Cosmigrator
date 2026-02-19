using System.Reflection;

namespace Cosmigrator;

/// <summary>
/// Discovers all classes implementing <see cref="IMigration"/> via reflection
/// and returns them sorted by <see cref="IMigration.Id"/>.
/// </summary>
public static class MigrationDiscovery
{
    /// <summary>
    /// Scans the specified assemblies for all concrete types that implement
    /// <see cref="IMigration"/>, instantiates them, and returns them sorted by Id.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for migration classes.</param>
    /// <returns>A list of migration instances sorted chronologically by Id.</returns>
    public static List<IMigration> DiscoverAll(params Assembly[] assemblies)
    {
        var migrationInterface = typeof(IMigration);

        var targetAssemblies = assemblies.Length > 0
            ? assemblies
            : new[] { Assembly.GetEntryAssembly()! };

        var migrations = targetAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => migrationInterface.IsAssignableFrom(t)
                        && t is { IsAbstract: false, IsInterface: false })
            .Select(t => (IMigration)Activator.CreateInstance(t)!)
            .OrderBy(m => m.Id)
            .ToList();

        return migrations;
    }
}
