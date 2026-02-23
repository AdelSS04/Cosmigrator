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
} from "lucide-react";

function Nav() {
  return (
    <nav className="sticky top-0 z-50 border-b border-neutral-800 bg-neutral-950/80 backdrop-blur-sm">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
        <Link href="/" className="text-lg font-semibold tracking-tight">
          Cosmigrator
        </Link>
        <div className="flex items-center gap-6 text-sm text-neutral-400">
          <a href="#features" className="hover:text-neutral-100 transition-colors">
            Features
          </a>
          <a href="#examples" className="hover:text-neutral-100 transition-colors">
            Examples
          </a>
          <a
            href="https://www.nuget.org/packages/Cosmigrator/"
            target="_blank"
            rel="noopener noreferrer"
            className="hover:text-neutral-100 transition-colors"
          >
            NuGet
          </a>
          <a
            href="https://github.com/AdelSS04/Cosmigrator"
            target="_blank"
            rel="noopener noreferrer"
            className="hover:text-neutral-100 transition-colors"
          >
            GitHub
          </a>
        </div>
      </div>
    </nav>
  );
}

function Hero() {
  return (
    <section className="mx-auto max-w-4xl px-6 pt-24 pb-20 text-center">
      <div className="mb-4 inline-block rounded-full border border-neutral-700 px-3 py-1 text-xs text-neutral-400">
        v1.0.4 &middot; .NET 8 / 9 / 10
      </div>
      <h1 className="text-4xl font-bold tracking-tight sm:text-5xl lg:text-6xl">
        Migrations for{" "}
        <span className="text-blue-400">Cosmos DB</span>
      </h1>
      <p className="mx-auto mt-4 max-w-2xl text-lg text-neutral-400">
        Version-controlled, reversible schema changes for your NoSQL documents.
        Like EF Core migrations, but built for Cosmos DB.
      </p>

      <div className="mt-8 inline-block rounded-lg border border-neutral-800 bg-neutral-900 px-5 py-3 font-mono text-sm text-neutral-300">
        <span className="text-neutral-500">$</span>{" "}
        dotnet add package Cosmigrator
      </div>

      <div className="mt-10 text-left">
        <p className="mb-2 text-xs font-medium uppercase tracking-wider text-neutral-500">
          Program.cs — that&apos;s it
        </p>
        <pre>
          <code>
            <span className="text-neutral-500">using</span>{" "}
            <span className="text-blue-300">System.Reflection</span>;{"\n"}
            <span className="text-neutral-500">using</span>{" "}
            <span className="text-blue-300">Cosmigrator</span>;{"\n"}
            {"\n"}
            <span className="text-neutral-500">await</span>{" "}
            <span className="text-yellow-300">MigrationHost</span>.
            <span className="text-green-300">RunAsync</span>(args,{" "}
            <span className="text-yellow-300">Assembly</span>.
            <span className="text-green-300">GetExecutingAssembly</span>());
          </code>
        </pre>
      </div>

      <div className="mt-8 flex items-center justify-center gap-4">
        <a
          href="#examples"
          className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-5 py-2.5 text-sm font-medium text-white hover:bg-blue-700 transition-colors"
        >
          See examples <ArrowRight className="h-4 w-4" />
        </a>
        <a
          href="https://github.com/AdelSS04/Cosmigrator"
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex items-center gap-2 rounded-lg border border-neutral-700 px-5 py-2.5 text-sm font-medium text-neutral-300 hover:border-neutral-600 hover:text-white transition-colors"
        >
          GitHub
        </a>
      </div>
    </section>
  );
}

function WhySection() {
  return (
    <section className="mx-auto max-w-5xl px-6 py-20">
      <h2 className="text-2xl font-bold tracking-tight text-center">
        The problem
      </h2>
      <p className="mt-2 text-center text-neutral-400 max-w-2xl mx-auto">
        Cosmos DB doesn&apos;t have migrations. Schema changes happen with ad-hoc scripts,
        manual patching, or hope. Cosmigrator gives you structure.
      </p>

      <div className="mt-12 grid gap-8 md:grid-cols-2">
        <div>
          <p className="mb-2 text-xs font-medium uppercase tracking-wider text-red-400">
            Before — ad-hoc script
          </p>
          <pre>
            <code>
              <span className="text-neutral-500">// Somewhere in a deployment script...</span>{"\n"}
              <span className="text-neutral-500">// No tracking, no rollback, no history</span>{"\n"}
              <span className="text-neutral-500">// Did this already run in prod?</span>{"\n"}
              <span className="text-neutral-500">// Who knows.</span>{"\n"}
              {"\n"}
              <span className="text-neutral-500">var</span> query = container.GetItemQueryIterator{"\n"}
              {"  "}&lt;dynamic&gt;(<span className="text-green-300">&quot;SELECT * FROM c&quot;</span>);{"\n"}
              <span className="text-neutral-500">while</span> (query.HasMoreResults){"\n"}
              {"{"}{"\n"}
              {"  "}<span className="text-neutral-500">var</span> batch = <span className="text-neutral-500">await</span> query.ReadNextAsync();{"\n"}
              {"  "}<span className="text-neutral-500">foreach</span> (<span className="text-neutral-500">var</span> doc <span className="text-neutral-500">in</span> batch){"\n"}
              {"  {"}{"\n"}
              {"    "}doc.age = <span className="text-yellow-300">0</span>;{"\n"}
              {"    "}<span className="text-neutral-500">await</span> container.UpsertItemAsync(doc);{"\n"}
              {"  }"}{"\n"}
              {"}"}
            </code>
          </pre>
        </div>

        <div>
          <p className="mb-2 text-xs font-medium uppercase tracking-wider text-green-400">
            After — Cosmigrator
          </p>
          <pre>
            <code>
              <span className="text-neutral-500">public class</span>{" "}
              <span className="text-yellow-300">AddAgeToUsers</span>{" "}
              : <span className="text-blue-300">IMigration</span>{"\n"}
              {"{"}{"\n"}
              {"  "}<span className="text-neutral-500">public</span> <span className="text-blue-300">string</span> Id =&gt; <span className="text-green-300">&quot;20250219_000001&quot;</span>;{"\n"}
              {"  "}<span className="text-neutral-500">public</span> <span className="text-blue-300">string</span> Name =&gt; <span className="text-green-300">&quot;AddAgeToUsers&quot;</span>;{"\n"}
              {"  "}<span className="text-neutral-500">public</span> <span className="text-blue-300">string</span> ContainerName =&gt; <span className="text-green-300">&quot;Users&quot;</span>;{"\n"}
              {"  "}<span className="text-neutral-500">public</span> <span className="text-blue-300">object?</span> DefaultValue =&gt; <span className="text-yellow-300">null</span>;{"\n"}
              {"\n"}
              {"  "}<span className="text-neutral-500">public async</span> Task <span className="text-green-300">UpAsync</span>(Container c, CosmosClient cl){"\n"}
              {"  {"}{"\n"}
              {"    "}<span className="text-neutral-500">var</span> helper = <span className="text-neutral-500">new</span> <span className="text-yellow-300">BulkOperationHelper</span>(logger);{"\n"}
              {"    "}<span className="text-neutral-500">var</span> docs = <span className="text-neutral-500">await</span> helper.ReadDocumentsAsync({"\n"}
              {"      "}c, <span className="text-green-300">&quot;SELECT * FROM c WHERE NOT IS_DEFINED(c.age)&quot;</span>);{"\n"}
              {"    "}<span className="text-neutral-500">foreach</span> (<span className="text-neutral-500">var</span> d <span className="text-neutral-500">in</span> docs) d[<span className="text-green-300">&quot;age&quot;</span>] = <span className="text-yellow-300">null</span>;{"\n"}
              {"    "}<span className="text-neutral-500">await</span> helper.BulkUpsertAsync(c, docs);{"\n"}
              {"  }"}{"\n"}
              {"\n"}
              {"  "}<span className="text-neutral-500">public async</span> Task <span className="text-green-300">DownAsync</span>(...) {"{ /* reverse */ }"}{"\n"}
              {"}"}
            </code>
          </pre>
        </div>
      </div>
    </section>
  );
}

const features = [
  {
    icon: Database,
    title: "Document transforms",
    description:
      "Add, remove, or rename properties across all documents in a container. Uses server-side SQL filtering to avoid loading unnecessary data.",
    code: `var docs = await helper.ReadDocumentsAsync(
  container,
  "SELECT * FROM c WHERE NOT IS_DEFINED(c.email)"
);

foreach (var doc in docs)
    doc["email"] = JsonValue.Create("");

await helper.BulkUpsertAsync(container, docs);`,
  },
  {
    icon: Zap,
    title: "Bulk operations with retry",
    description:
      "Built-in BulkOperationHelper with configurable batch size, automatic 429 retry with exponential backoff, and progress logging.",
    code: `var helper = new BulkOperationHelper(
    logger,
    batchSize: 100,
    maxRetries: 5,
    baseDelay: TimeSpan.FromSeconds(1)
);

// Read, transform, upsert — with automatic batching
await helper.BulkUpsertAsync(container, documents);`,
  },
  {
    icon: RotateCcw,
    title: "Rollback support",
    description:
      "Every migration implements UpAsync and DownAsync. Roll back the last N migrations from the CLI when something goes wrong.",
    code: `// Apply all pending migrations
dotnet run

// Undo the last migration
dotnet run -- rollback

// Undo the last 3 migrations
dotnet run -- rollback --steps 3`,
  },
  {
    icon: Layers,
    title: "Index & container changes",
    description:
      "Modify indexing policies, add composite indexes, or swap containers to change unique key policies — all versioned and reversible.",
    code: `// Add composite index to existing container
var response = await container.ReadContainerAsync();
var properties = response.Resource;

properties.IndexingPolicy.CompositeIndexes.Add(
    new Collection<CompositePath>
    {
        new() { Path = "/lastName",  Order = Ascending },
        new() { Path = "/firstName", Order = Ascending }
    }
);

await container.ReplaceContainerAsync(properties);`,
  },
  {
    icon: Search,
    title: "Status & discovery",
    description:
      "Migrations are discovered automatically via reflection. Check which migrations are applied, pending, or rolled back at any time.",
    code: `// See all discovered migrations
dotnet run -- list

// Check applied vs pending vs rolled back
dotnet run -- status`,
  },
  {
    icon: GitBranch,
    title: "History tracking",
    description:
      "All applied migrations are recorded in a __MigrationHistory container with timestamps and status. No migration runs twice.",
    code: `// Stored in Cosmos DB "__MigrationHistory" container
{
    "id": "20250219_000001",
    "name": "AddAgeToUsers",
    "appliedAt": "2025-02-19T14:30:00Z",
    "status": "Applied"
}`,
  },
];

function Features() {
  return (
    <section id="features" className="mx-auto max-w-5xl px-6 py-20">
      <h2 className="text-2xl font-bold tracking-tight text-center">
        What you can do
      </h2>
      <p className="mt-2 text-center text-neutral-400">
        Real migration scenarios, not toy examples.
      </p>

      <div className="mt-12 space-y-16">
        {features.map((f, i) => {
          const Icon = f.icon;
          const isEven = i % 2 === 1;
          return (
            <div
              key={f.title}
              className={`flex flex-col gap-8 ${
                isEven ? "md:flex-row-reverse" : "md:flex-row"
              } items-start`}
            >
              <div className="flex-1">
                <div className="flex items-center gap-3 mb-3">
                  <div className="flex h-9 w-9 items-center justify-center rounded-lg border border-neutral-800 bg-neutral-900">
                    <Icon className="h-4 w-4 text-blue-400" />
                  </div>
                  <h3 className="text-lg font-semibold">{f.title}</h3>
                </div>
                <p className="text-neutral-400 leading-relaxed">
                  {f.description}
                </p>
              </div>
              <div className="flex-1 w-full">
                <pre className="text-sm">
                  <code>{f.code}</code>
                </pre>
              </div>
            </div>
          );
        })}
      </div>
    </section>
  );
}

function IntegrationExamples() {
  return (
    <section id="examples" className="mx-auto max-w-5xl px-6 py-20">
      <h2 className="text-2xl font-bold tracking-tight text-center">
        Integrations
      </h2>
      <p className="mt-2 text-center text-neutral-400">
        Drop into your existing deployment pipeline.
      </p>

      <div className="mt-12 grid gap-8 md:grid-cols-2">
        <div>
          <p className="mb-2 text-xs font-medium uppercase tracking-wider text-neutral-500">
            Kubernetes init container
          </p>
          <pre className="text-sm">
            <code>
              {`initContainers:
  - name: migrations
    image: myregistry/myservice-migrations:latest
    command: ["dotnet", "MyService.Migrations.dll"]
    env:
      - name: CosmosDb__ConnectionString
        valueFrom:
          secretKeyRef:
            name: cosmos-secrets
            key: connection-string`}
            </code>
          </pre>
        </div>

        <div>
          <p className="mb-2 text-xs font-medium uppercase tracking-wider text-neutral-500">
            Custom host setup
          </p>
          <pre className="text-sm">
            <code>
              {`var host = Host.CreateDefaultBuilder(args)
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
    host.Services.GetRequiredService<IConfiguration>(),
    host.Services.GetRequiredService<ILoggerFactory>(),
    args, Assembly.GetExecutingAssembly());`}
            </code>
          </pre>
        </div>
      </div>
    </section>
  );
}

function Ecosystem() {
  const packages = [
    {
      name: "Cosmigrator",
      description:
        "Core library — migration runner, history tracking, bulk operations, and CLI.",
      nuget: "https://www.nuget.org/packages/Cosmigrator/",
      frameworks: ".NET 8 / 9 / 10",
    },
  ];

  return (
    <section className="mx-auto max-w-5xl px-6 py-20">
      <h2 className="text-2xl font-bold tracking-tight text-center">
        Packages
      </h2>
      <p className="mt-2 text-center text-neutral-400">
        Available on NuGet.
      </p>

      <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {packages.map((pkg) => (
          <a
            key={pkg.name}
            href={pkg.nuget}
            target="_blank"
            rel="noopener noreferrer"
            className="group rounded-lg border border-neutral-800 bg-neutral-900/50 p-5 hover:border-neutral-700 transition-colors"
          >
            <div className="flex items-center gap-2 mb-2">
              <Package className="h-4 w-4 text-blue-400" />
              <h3 className="font-semibold">{pkg.name}</h3>
            </div>
            <p className="text-sm text-neutral-400">{pkg.description}</p>
            <p className="mt-3 text-xs text-neutral-500">{pkg.frameworks}</p>
          </a>
        ))}
      </div>
    </section>
  );
}

function CTA() {
  return (
    <section className="mx-auto max-w-3xl px-6 py-20 text-center">
      <h2 className="text-2xl font-bold tracking-tight">
        Stop writing ad-hoc migration scripts
      </h2>
      <p className="mt-3 text-neutral-400">
        Install the package, implement <code>IMigration</code>, run{" "}
        <code>dotnet run</code>.
      </p>
      <div className="mt-8 inline-block rounded-lg border border-neutral-800 bg-neutral-900 px-5 py-3 font-mono text-sm text-neutral-300">
        <span className="text-neutral-500">$</span>{" "}
        dotnet add package Cosmigrator
      </div>
      <div className="mt-6 flex items-center justify-center gap-4">
        <a
          href="https://github.com/AdelSS04/Cosmigrator"
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex items-center gap-2 rounded-lg bg-blue-600 px-5 py-2.5 text-sm font-medium text-white hover:bg-blue-700 transition-colors"
        >
          Get started <ArrowRight className="h-4 w-4" />
        </a>
      </div>
    </section>
  );
}

function Footer() {
  return (
    <footer className="border-t border-neutral-800 py-8">
      <div className="mx-auto flex max-w-6xl items-center justify-between px-6 text-sm text-neutral-500">
        <span>MIT License</span>
        <div className="flex items-center gap-4">
          <a
            href="https://github.com/AdelSS04/Cosmigrator"
            target="_blank"
            rel="noopener noreferrer"
            className="hover:text-neutral-300 transition-colors"
          >
            GitHub
          </a>
          <a
            href="https://www.nuget.org/packages/Cosmigrator/"
            target="_blank"
            rel="noopener noreferrer"
            className="hover:text-neutral-300 transition-colors"
          >
            NuGet
          </a>
        </div>
      </div>
    </footer>
  );
}

export default function Page() {
  return (
    <>
      <Nav />
      <main>
        <Hero />
        <WhySection />
        <Features />
        <IntegrationExamples />
        <Ecosystem />
        <CTA />
      </main>
      <Footer />
    </>
  );
}
