import Link from "next/link";
import {
  ArrowRight,
  Database,
  RotateCcw,
  Terminal,
  Layers,
  Zap,
  GitBranch,
  Package,
  Search,
  Shield,
  Github,
  BookOpen,
  ExternalLink,
  CheckCircle,
} from "lucide-react";

const NAV_LINKS = [
  { href: "#why", label: "Why Cosmigrator" },
  { href: "#features", label: "Features" },
  { href: "#examples", label: "Examples" },
  { href: "https://adelss04.github.io/Cosmigrator/", label: "Docs" },
];

export default function Page() {
  return (
    <div className="min-h-screen bg-white text-gray-900">
      {/* ───── NAV ───── */}
      <nav className="fixed top-0 inset-x-0 z-50 bg-white/80 backdrop-blur-md border-b border-gray-100">
        <div className="max-w-6xl mx-auto flex items-center justify-between h-16 px-6">
          <Link href="/" className="flex items-center gap-2.5">
            <Database className="w-6 h-6 text-blue-600" />
            <span className="text-lg font-semibold tracking-tight">Cosmigrator</span>
          </Link>

          <div className="hidden md:flex items-center gap-8">
            {NAV_LINKS.map((l) => (
              <Link
                key={l.href}
                href={l.href}
                className="text-sm text-gray-500 hover:text-gray-900 transition-colors"
              >
                {l.label}
              </Link>
            ))}
          </div>

          <div className="flex items-center gap-3">
            <Link
              href="https://github.com/AdelSS04/Cosmigrator"
              target="_blank"
              className="hidden sm:flex items-center gap-1.5 text-sm text-gray-500 hover:text-gray-900 transition-colors"
            >
              <Github className="w-4 h-4" />
              GitHub
            </Link>
            <Link
              href="https://www.nuget.org/packages/Cosmigrator"
              target="_blank"
              className="text-sm font-medium px-4 py-2 rounded-lg bg-gray-900 text-white hover:bg-gray-800 transition-colors"
            >
              Install
            </Link>
          </div>
        </div>
      </nav>

      {/* ───── HERO ───── */}
      <section className="pt-32 pb-20 px-6">
        <div className="max-w-4xl mx-auto text-center">
          <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full bg-blue-50 text-blue-700 text-xs font-medium mb-8 ring-1 ring-blue-200/60">
            <span className="w-1.5 h-1.5 rounded-full bg-blue-500" />
            v1.0.4 &middot; .NET 8 / 9 / 10
          </div>

          <h1 className="text-5xl sm:text-6xl lg:text-7xl font-extrabold tracking-tight leading-[1.08] mb-6">
            Migrations for
            <br />
            <span className="text-transparent bg-clip-text bg-gradient-to-r from-blue-600 to-violet-600">
              Azure Cosmos DB
            </span>
          </h1>

          <p className="text-lg sm:text-xl text-gray-500 max-w-2xl mx-auto mb-10 leading-relaxed">
            Version-controlled, reversible schema changes for your NoSQL documents.
            Like EF Core migrations, but built for{" "}
            <code className="text-gray-700 bg-gray-100 px-1.5 py-0.5 rounded text-[0.9em]">
              Cosmos DB
            </code>.
          </p>

          <div className="flex flex-col sm:flex-row items-center justify-center gap-4 mb-14">
            {/* Install command */}
            <div className="flex items-center gap-3 px-5 py-3 rounded-xl bg-gray-950 text-gray-300 font-mono text-sm select-all">
              <Terminal className="w-4 h-4 text-gray-500 shrink-0" />
              dotnet add package Cosmigrator
            </div>
            <Link
              href="https://adelss04.github.io/Cosmigrator/"
              target="_blank"
              className="flex items-center gap-2 px-5 py-3 rounded-xl text-sm font-medium text-gray-700 border border-gray-200 hover:border-gray-300 hover:bg-gray-50 transition-all"
            >
              <BookOpen className="w-4 h-4" />
              Read the docs
            </Link>
          </div>

          {/* Hero code block */}
          <div className="max-w-2xl mx-auto text-left rounded-2xl bg-gray-950 ring-1 ring-white/10 overflow-hidden shadow-2xl">
            <div className="flex items-center gap-2 px-5 py-3 border-b border-white/5">
              <span className="w-3 h-3 rounded-full bg-red-500/70" />
              <span className="w-3 h-3 rounded-full bg-yellow-500/70" />
              <span className="w-3 h-3 rounded-full bg-green-500/70" />
              <span className="ml-3 text-xs text-gray-500 font-mono">Program.cs</span>
            </div>
            <pre className="p-6 text-sm leading-relaxed overflow-x-auto !bg-transparent !border-0 !rounded-none !p-6">
              <code className="text-gray-300">{`using System.Reflection;
using Cosmigrator;

await MigrationHost.RunAsync(args, Assembly.GetExecutingAssembly());

// That's it. Discovers migrations, connects to Cosmos DB,
// applies pending changes, tracks history — automatically.`}</code>
            </pre>
          </div>
        </div>
      </section>

      {/* ───── WHY COSMIGRATOR ───── */}
      <section id="why" className="py-24 px-6 bg-gray-50 border-y border-gray-100">
        <div className="max-w-6xl mx-auto">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">
            Why not ad-hoc scripts?
          </h2>
          <p className="text-gray-500 mb-14 max-w-2xl">
            Cosmos DB doesn&apos;t have built-in migrations. Schema changes happen with
            ad-hoc scripts, manual patching, or hope. Cosmigrator gives you structure.{" "}
            <Link href="https://adelss04.github.io/Cosmigrator/core-concepts/how-it-works" className="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-1">Learn more <ArrowRight className="w-3.5 h-3.5" /></Link>
          </p>

          <div className="grid lg:grid-cols-2 gap-6">
            {/* BEFORE */}
            <div className="rounded-2xl border border-red-200 bg-white p-8">
              <div className="flex items-center gap-2 mb-5">
                <span className="w-2.5 h-2.5 rounded-full bg-red-500" />
                <span className="text-sm font-semibold text-red-600 uppercase tracking-wide">
                  Ad-hoc script
                </span>
              </div>
              <pre className="text-sm leading-relaxed text-gray-800 overflow-x-auto !bg-transparent !border-0 !p-0">
                <code>{`// Somewhere in a deployment script...
// No tracking, no rollback, no history
// Did this already run in prod? Who knows.

var query = container.GetItemQueryIterator
    <dynamic>("SELECT * FROM c");
while (query.HasMoreResults)
{
    var batch = await query.ReadNextAsync();
    foreach (var doc in batch)
    {
        doc.age = 0;
        await container.UpsertItemAsync(doc);
    }
}
// Good luck rolling this back.`}</code>
              </pre>
            </div>

            {/* AFTER */}
            <div className="rounded-2xl border border-emerald-200 bg-white p-8">
              <div className="flex items-center gap-2 mb-5">
                <span className="w-2.5 h-2.5 rounded-full bg-emerald-500" />
                <span className="text-sm font-semibold text-emerald-600 uppercase tracking-wide">
                  With Cosmigrator
                </span>
              </div>
              <pre className="text-sm leading-relaxed text-gray-800 overflow-x-auto !bg-transparent !border-0 !p-0">
                <code>{`public class AddAgeToUsers : IMigration
{
    public string Id => "20250219_000001";
    public string Name => "AddAgeToUsers";
    public string ContainerName => "Users";

    public async Task UpAsync(Container c, CosmosClient cl)
    {
        var helper = new BulkOperationHelper(logger);
        var docs = await helper.ReadDocumentsAsync(
            c, "SELECT * FROM c WHERE NOT IS_DEFINED(c.age)");
        foreach (var d in docs) d["age"] = null;
        await helper.BulkUpsertAsync(c, docs);
    }

    public async Task DownAsync(...) { /* reverse */ }
}`}</code>
              </pre>
            </div>
          </div>

          {/* Value props */}
          <div className="grid sm:grid-cols-3 gap-8 mt-16">
            <div>
              <div className="text-sm font-semibold text-gray-900 mb-2">Tracked & versioned</div>
              <p className="text-sm text-gray-500 leading-relaxed">
                Every migration gets a unique ID and timestamp.
                Applied state is stored in a <code className="text-gray-700 bg-gray-100 px-1 rounded text-xs">__MigrationHistory</code> container.
              </p>
            </div>
            <div>
              <div className="text-sm font-semibold text-gray-900 mb-2">Rollback built in</div>
              <p className="text-sm text-gray-500 leading-relaxed">
                Every migration has <code className="text-gray-700 bg-gray-100 px-1 rounded text-xs">UpAsync</code> and{" "}
                <code className="text-gray-700 bg-gray-100 px-1 rounded text-xs">DownAsync</code>.
                Roll back N steps with a single CLI command.
              </p>
            </div>
            <div>
              <div className="text-sm font-semibold text-gray-900 mb-2">Auto-discovery</div>
              <p className="text-sm text-gray-500 leading-relaxed">
                Migrations are discovered via reflection.
                Just implement <code className="text-gray-700 bg-gray-100 px-1 rounded text-xs">IMigration</code> and the runner finds them.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* ───── FEATURES ───── */}
      <section id="features" className="py-24 px-6">
        <div className="max-w-6xl mx-auto">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">
            What you can do
          </h2>
          <p className="text-gray-500 mb-14 max-w-2xl">
            Real migration scenarios, not toy examples. Every feature works with the full Cosmos DB SDK.{" "}
            <Link href="https://adelss04.github.io/Cosmigrator/migrations/add-property" className="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-1">See migration guides <ArrowRight className="w-3.5 h-3.5" /></Link>
          </p>

          <div className="grid lg:grid-cols-[1fr_1.2fr] gap-12 items-start">
            {/* Feature list */}
            <div className="space-y-6">
              {[
                {
                  icon: Database,
                  op: "Document transforms",
                  desc: "Add, remove, or rename properties across all documents. Uses SQL filtering to avoid loading unnecessary data.",
                  docsUrl: "https://adelss04.github.io/Cosmigrator/migrations/add-property",
                },
                {
                  icon: Zap,
                  op: "Bulk operations",
                  desc: "Built-in BulkOperationHelper with configurable batch size, automatic 429 retry with exponential backoff.",
                  docsUrl: "https://adelss04.github.io/Cosmigrator/api-reference/bulk-operation-helper",
                },
                {
                  icon: RotateCcw,
                  op: "Rollback support",
                  desc: "Every migration has UpAsync and DownAsync. Roll back the last N migrations from the CLI.",
                  docsUrl: "https://adelss04.github.io/Cosmigrator/getting-started/cli-commands",
                },
                {
                  icon: Layers,
                  op: "Index & container changes",
                  desc: "Modify indexing policies, add composite indexes, or swap containers — all versioned and reversible.",
                  docsUrl: "https://adelss04.github.io/Cosmigrator/migrations/composite-index",
                },
                {
                  icon: Search,
                  op: "Status & discovery",
                  desc: "Check which migrations are applied, pending, or rolled back at any time via the CLI.",
                  docsUrl: "https://adelss04.github.io/Cosmigrator/api-reference/migration-discovery",
                },
                {
                  icon: GitBranch,
                  op: "History tracking",
                  desc: "All applied migrations are recorded with timestamps and status. No migration runs twice.",
                  docsUrl: "https://adelss04.github.io/Cosmigrator/core-concepts/history-tracking",
                },
              ].map(({ icon: Icon, op, desc, docsUrl }) => (
                <div key={op} className="flex gap-4">
                  <div className="shrink-0 flex h-9 w-9 items-center justify-center rounded-lg bg-blue-50 ring-1 ring-blue-100">
                    <Icon className="w-4 h-4 text-blue-600" />
                  </div>
                  <div>
                    <div className="text-sm font-semibold text-gray-900 mb-1">{op}</div>
                    <p className="text-sm text-gray-500 leading-relaxed">
                      {desc}{" "}
                      <Link href={docsUrl} target="_blank" className="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-0.5 text-xs">
                        Docs <ArrowRight className="w-3 h-3" />
                      </Link>
                    </p>
                  </div>
                </div>
              ))}
            </div>

            {/* Feature code example */}
            <div className="rounded-2xl bg-gray-950 ring-1 ring-white/10 overflow-hidden shadow-xl">
              <div className="flex items-center gap-2 px-5 py-3 border-b border-white/5">
                <span className="w-3 h-3 rounded-full bg-red-500/70" />
                <span className="w-3 h-3 rounded-full bg-yellow-500/70" />
                <span className="w-3 h-3 rounded-full bg-green-500/70" />
                <span className="ml-3 text-xs text-gray-500 font-mono">AddCompositeIndex.cs</span>
              </div>
              <pre className="p-6 text-sm leading-relaxed overflow-x-auto !bg-transparent !border-0 !rounded-none !p-6">
                <code className="text-gray-300">{`public class AddCompositeIndexToUsers : IMigration
{
    public string Id => "20240101_000005";
    public string Name => "AddCompositeIndexToUsers";
    public string ContainerName => "Users";

    public async Task UpAsync(
        Container container, CosmosClient client)
    {
        var response =
            await container.ReadContainerAsync();
        var properties = response.Resource;

        properties.IndexingPolicy.CompositeIndexes
            .Add(new Collection<CompositePath>
        {
            new() { Path = "/lastName",
                     Order = Ascending },
            new() { Path = "/firstName",
                     Order = Ascending }
        });

        await container
            .ReplaceContainerAsync(properties);
    }

    public async Task DownAsync(
        Container container, CosmosClient client)
    {
        // Remove the composite index
        var response =
            await container.ReadContainerAsync();
        var props = response.Resource;
        props.IndexingPolicy.CompositeIndexes
            .RemoveAt(props.IndexingPolicy
                .CompositeIndexes.Count - 1);
        await container
            .ReplaceContainerAsync(props);
    }
}`}</code>
              </pre>
            </div>
          </div>
        </div>
      </section>

      {/* ───── CLI ───── */}
      <section className="py-24 px-6 bg-gray-50 border-y border-gray-100">
        <div className="max-w-6xl mx-auto">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">
            Simple CLI
          </h2>
          <p className="text-gray-500 mb-14 max-w-2xl">
            Run migrations, roll back changes, and check status — all from one command with no configuration needed.{" "}
            <Link href="https://adelss04.github.io/Cosmigrator/getting-started/cli-commands" className="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-1">CLI reference <ArrowRight className="w-3.5 h-3.5" /></Link>
          </p>

          <div className="grid lg:grid-cols-2 gap-6">
            <div className="rounded-2xl bg-gray-950 ring-1 ring-white/10 overflow-hidden">
              <div className="px-5 py-3 border-b border-white/5 text-xs text-gray-500 font-mono">
                Apply &amp; status
              </div>
              <pre className="p-6 text-sm leading-relaxed overflow-x-auto !bg-transparent !border-0 !rounded-none !p-6">
                <code className="text-gray-300">{`# Apply all pending migrations
$ dotnet run
  ✓ 20240101_000001 — AddAgePropertyToUsers
  ✓ 20240101_000002 — RemoveMiddleNameProperty
  ✓ 20240101_000003 — RenameUserNameToDisplayName
  Applied 3 migrations successfully.

# List all discovered migrations
$ dotnet run -- list
  20240101_000001  AddAgePropertyToUsers
  20240101_000002  RemoveMiddleNameProperty
  20240101_000003  RenameUserNameToDisplayName
  20240101_000004  AddUniqueKeyPolicyToOrders
  20240101_000005  AddCompositeIndexToUsers

# Check current status
$ dotnet run -- status`}</code>
              </pre>
            </div>

            <div className="rounded-2xl bg-gray-950 ring-1 ring-white/10 overflow-hidden">
              <div className="px-5 py-3 border-b border-white/5 text-xs text-gray-500 font-mono">
                Rollback
              </div>
              <pre className="p-6 text-sm leading-relaxed overflow-x-auto !bg-transparent !border-0 !rounded-none !p-6">
                <code className="text-gray-300">{`# Undo the last migration
$ dotnet run -- rollback
  ↺ Rolling back 20240101_000003
  ✓ RenameUserNameToDisplayName — rolled back

# Undo the last 3 migrations
$ dotnet run -- rollback --steps 3
  ↺ Rolling back 20240101_000003
  ↺ Rolling back 20240101_000002
  ↺ Rolling back 20240101_000001
  ✓ Rolled back 3 migrations

# Configuration via appsettings.json
{
    "CosmosDb": {
        "ConnectionString": "AccountEndpoint=...",
        "DatabaseName": "MyDatabase"
    }
}`}</code>
              </pre>
            </div>
          </div>
        </div>
      </section>

      {/* ───── INTEGRATIONS ───── */}
      <section id="examples" className="py-24 px-6">
        <div className="max-w-6xl mx-auto">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">
            Drop into your pipeline
          </h2>
          <p className="text-gray-500 mb-14 max-w-2xl">
            Run migrations as a Kubernetes init container, in a CI/CD pipeline,
            or with a custom host — Cosmigrator fits your workflow.{" "}
            <Link href="https://adelss04.github.io/Cosmigrator/integrations/ci-cd" className="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-1">Integration guides <ArrowRight className="w-3.5 h-3.5" /></Link>
          </p>

          <div className="grid lg:grid-cols-2 gap-6">
            <div className="rounded-2xl bg-gray-950 ring-1 ring-white/10 overflow-hidden">
              <div className="px-5 py-3 border-b border-white/5 text-xs text-gray-500 font-mono">
                Kubernetes init container
              </div>
              <pre className="p-6 text-sm leading-relaxed overflow-x-auto !bg-transparent !border-0 !rounded-none !p-6">
                <code className="text-gray-300">{`initContainers:
  - name: migrations
    image: myregistry/myservice:latest
    command: ["dotnet", "Migrations.dll"]
    env:
      - name: CosmosDb__ConnectionString
        valueFrom:
          secretKeyRef:
            name: cosmos-secrets
            key: connection-string
      - name: CosmosDb__DatabaseName
        value: "MyDatabase"`}</code>
              </pre>
            </div>

            <div className="rounded-2xl bg-gray-950 ring-1 ring-white/10 overflow-hidden">
              <div className="px-5 py-3 border-b border-white/5 text-xs text-gray-500 font-mono">
                Custom host setup
              </div>
              <pre className="p-6 text-sm leading-relaxed overflow-x-auto !bg-transparent !border-0 !rounded-none !p-6">
                <code className="text-gray-300">{`var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((_, config) =>
    {
        config
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddCommandLine(args);
    })
    .UseSerilog()
    .Build();

await MigrationHost.RunAsync(
    host.Services
        .GetRequiredService<IConfiguration>(),
    host.Services
        .GetRequiredService<ILoggerFactory>(),
    args,
    Assembly.GetExecutingAssembly());`}</code>
              </pre>
            </div>
          </div>
        </div>
      </section>

      {/* ───── MIGRATION HISTORY ───── */}
      <section className="py-24 px-6 bg-gray-50 border-y border-gray-100">
        <div className="max-w-6xl mx-auto">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">
            Full audit trail
          </h2>
          <p className="text-gray-500 mb-14 max-w-2xl">
            Every migration is recorded in a <code className="text-gray-700 bg-gray-100 px-1.5 py-0.5 rounded text-[0.9em]">__MigrationHistory</code> container.
            Know exactly what ran, when, and its current status.{" "}
            <Link href="https://adelss04.github.io/Cosmigrator/core-concepts/history-tracking" className="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-1">History tracking docs <ArrowRight className="w-3.5 h-3.5" /></Link>
          </p>

          <div className="grid sm:grid-cols-3 gap-6">
            {[
              {
                status: "Applied",
                color: "emerald",
                record: `{
  "id": "20240101_000001",
  "name": "AddAgePropertyToUsers",
  "appliedAt": "2025-01-15T14:30:00Z",
  "status": "Applied"
}`,
              },
              {
                status: "Applied",
                color: "emerald",
                record: `{
  "id": "20240101_000002",
  "name": "RemoveMiddleNameProperty",
  "appliedAt": "2025-01-15T14:30:01Z",
  "status": "Applied"
}`,
              },
              {
                status: "RolledBack",
                color: "amber",
                record: `{
  "id": "20240101_000003",
  "name": "RenameUserNameToDisplayName",
  "appliedAt": "2025-01-15T14:30:02Z",
  "rolledBackAt": "2025-01-16T09:00:00Z",
  "status": "RolledBack"
}`,
              },
            ].map((item) => (
              <div
                key={item.record}
                className={`rounded-2xl border bg-white p-6 ${
                  item.color === "emerald"
                    ? "border-emerald-200"
                    : "border-amber-200"
                }`}
              >
                <div className="flex items-center gap-2 mb-4">
                  <span
                    className={`w-2 h-2 rounded-full ${
                      item.color === "emerald" ? "bg-emerald-500" : "bg-amber-500"
                    }`}
                  />
                  <span
                    className={`text-xs font-semibold uppercase tracking-wide ${
                      item.color === "emerald" ? "text-emerald-600" : "text-amber-600"
                    }`}
                  >
                    {item.status}
                  </span>
                </div>
                <pre className="text-xs leading-relaxed text-gray-700 overflow-x-auto !bg-transparent !border-0 !p-0">
                  <code>{item.record}</code>
                </pre>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ───── ECOSYSTEM ───── */}
      <section className="py-24 px-6">
        <div className="max-w-6xl mx-auto">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">
            Ecosystem
          </h2>
          <p className="text-gray-500 mb-14 max-w-2xl">
            A single NuGet package with everything you need. No extra dependencies required.{" "}
            <Link href="https://adelss04.github.io/Cosmigrator/api-reference/migration-host" className="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-1">API reference <ArrowRight className="w-3.5 h-3.5" /></Link>
          </p>

          <div className="grid sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {[
              {
                name: "Cosmigrator",
                desc: "Core library — migration runner, history tracking, bulk operations, and CLI.",
                core: true,
                docsUrl: "https://adelss04.github.io/Cosmigrator/getting-started/installation",
              },
              {
                name: "IMigration",
                desc: "The migration interface. Implement UpAsync and DownAsync for each schema change.",
                core: false,
                docsUrl: "https://adelss04.github.io/Cosmigrator/api-reference/imigration",
              },
              {
                name: "BulkOperationHelper",
                desc: "Read, transform, and upsert documents in batches with automatic retry on 429.",
                core: false,
                docsUrl: "https://adelss04.github.io/Cosmigrator/api-reference/bulk-operation-helper",
              },
              {
                name: "MigrationHistory",
                desc: "Tracks applied and rolled-back migrations in a dedicated Cosmos DB container.",
                core: false,
                docsUrl: "https://adelss04.github.io/Cosmigrator/api-reference/migration-history",
              },
            ].map((pkg) => (
              <div
                key={pkg.name}
                className="group flex flex-col p-5 rounded-xl border border-gray-200 hover:border-blue-200 hover:shadow-md transition-all bg-white"
              >
                <div className="flex items-center justify-between mb-3">
                  <Package className="w-5 h-5 text-gray-400 group-hover:text-blue-500 transition-colors" />
                  {pkg.core && (
                    <span className="text-[10px] font-semibold uppercase tracking-wider text-blue-600 bg-blue-50 px-2 py-0.5 rounded-full">
                      Core
                    </span>
                  )}
                </div>
                <div className="text-sm font-semibold text-gray-900 mb-1.5">{pkg.name}</div>
                <p className="text-xs text-gray-500 leading-relaxed">{pkg.desc}</p>
                <div className="mt-auto pt-3 flex items-center gap-3 text-xs">
                  {pkg.core && (
                    <Link
                      href="https://www.nuget.org/packages/Cosmigrator"
                      target="_blank"
                      className="flex items-center gap-1 text-gray-400 group-hover:text-blue-500 transition-colors"
                    >
                      NuGet <ExternalLink className="w-3 h-3" />
                    </Link>
                  )}
                  {pkg.docsUrl && (
                    <Link
                      href={pkg.docsUrl}
                      target="_blank"
                      className="flex items-center gap-1 text-gray-400 group-hover:text-blue-500 transition-colors"
                    >
                      Docs <BookOpen className="w-3 h-3" />
                    </Link>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ───── QUICK START ───── */}
      <section className="py-24 px-6 bg-gray-50 border-y border-gray-100">
        <div className="max-w-4xl mx-auto">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4 text-center">
            Up and running in 3 steps
          </h2>
          <p className="text-gray-500 mb-14 max-w-2xl mx-auto text-center">
            No complex setup. No boilerplate. Just migrations.{" "}
            <Link href="https://adelss04.github.io/Cosmigrator/getting-started/quick-start" className="text-blue-600 hover:text-blue-700 font-medium inline-flex items-center gap-1">Full quick start guide <ArrowRight className="w-3.5 h-3.5" /></Link>
          </p>

          <div className="space-y-8">
            {[
              {
                step: "1",
                title: "Install the package",
                code: "dotnet add package Cosmigrator",
              },
              {
                step: "2",
                title: "Create a migration",
                code: `public class AddEmailToUsers : IMigration
{
    public string Id => "20250601_000001";
    public string Name => "AddEmailToUsers";
    public string ContainerName => "Users";
    public object? DefaultValue => "";

    public async Task UpAsync(Container c, CosmosClient cl)
    {
        var helper = new BulkOperationHelper(logger);
        var docs = await helper.ReadDocumentsAsync(
            c, "SELECT * FROM c WHERE NOT IS_DEFINED(c.email)");
        foreach (var d in docs) d["email"] = "";
        await helper.BulkUpsertAsync(c, docs);
    }

    public Task DownAsync(Container c, CosmosClient cl) =>
        Task.CompletedTask; // reverse logic
}`,
              },
              {
                step: "3",
                title: "Run it",
                code: `// Program.cs — one line
await MigrationHost.RunAsync(args, Assembly.GetExecutingAssembly());`,
              },
            ].map((s) => (
              <div key={s.step} className="flex gap-6 items-start">
                <div className="shrink-0 flex h-10 w-10 items-center justify-center rounded-full bg-blue-600 text-white text-sm font-bold">
                  {s.step}
                </div>
                <div className="flex-1">
                  <h3 className="text-lg font-semibold mb-3">{s.title}</h3>
                  <div className="rounded-2xl bg-gray-950 ring-1 ring-white/10 overflow-hidden">
                    <pre className="p-5 text-sm leading-relaxed overflow-x-auto !bg-transparent !border-0 !rounded-none !p-5">
                      <code className="text-gray-300">{s.code}</code>
                    </pre>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ───── CTA ───── */}
      <section className="py-24 px-6 bg-gray-950 text-white">
        <div className="max-w-3xl mx-auto text-center">
          <h2 className="text-3xl sm:text-4xl font-bold tracking-tight mb-4">
            Stop writing ad-hoc migration scripts
          </h2>
          <p className="text-gray-400 mb-10 text-lg">
            Install Cosmigrator, implement <code className="text-gray-200 bg-white/10 px-1.5 py-0.5 rounded text-[0.9em]">IMigration</code>,
            run <code className="text-gray-200 bg-white/10 px-1.5 py-0.5 rounded text-[0.9em]">dotnet run</code>.
          </p>

          <div className="flex flex-col sm:flex-row items-center justify-center gap-4">
            <Link
              href="https://github.com/AdelSS04/Cosmigrator"
              target="_blank"
              className="flex items-center gap-2.5 px-6 py-3.5 rounded-xl bg-white text-gray-900 font-semibold hover:bg-gray-100 transition-colors text-sm"
            >
              <Github className="w-5 h-5" />
              Star on GitHub
              <ArrowRight className="w-4 h-4" />
            </Link>
            <Link
              href="https://adelss04.github.io/Cosmigrator/"
              target="_blank"
              className="flex items-center gap-2.5 px-6 py-3.5 rounded-xl ring-1 ring-white/20 text-white font-semibold hover:bg-white/5 transition-colors text-sm"
            >
              <BookOpen className="w-5 h-5" />
              Documentation
            </Link>
          </div>
        </div>
      </section>

      {/* ───── FOOTER ───── */}
      <footer className="py-12 px-6 border-t border-gray-100">
        <div className="max-w-6xl mx-auto flex flex-col sm:flex-row items-center justify-between gap-6">
          <div className="flex items-center gap-2.5">
            <Database className="w-5 h-5 text-blue-600" />
            <span className="text-sm text-gray-400">
              &copy; {new Date().getFullYear()} Cosmigrator &middot; MIT License
            </span>
          </div>
          <div className="flex items-center gap-6 text-sm text-gray-400">
            <Link
              href="https://adelss04.github.io/Cosmigrator/"
              target="_blank"
              className="hover:text-gray-700 transition-colors"
            >
              Docs
            </Link>
            <Link
              href="https://github.com/AdelSS04/Cosmigrator"
              target="_blank"
              className="hover:text-gray-700 transition-colors"
            >
              GitHub
            </Link>
            <Link
              href="https://www.nuget.org/packages/Cosmigrator"
              target="_blank"
              className="hover:text-gray-700 transition-colors"
            >
              NuGet
            </Link>
            <Link
              href="https://github.com/AdelSS04/Cosmigrator/discussions"
              target="_blank"
              className="hover:text-gray-700 transition-colors"
            >
              Discussions
            </Link>
          </div>
        </div>
      </footer>
    </div>
  );
}
