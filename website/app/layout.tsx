import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "Cosmigrator — Migration framework for Azure Cosmos DB",
  description:
    "Version-controlled, reversible schema migrations for Azure Cosmos DB. Add, remove, rename properties, update indexes, and swap containers — all from C# code. Supports .NET 8, 9, and 10.",
  keywords: [
    "cosmos db migrations",
    "azure cosmos db schema migration",
    "cosmos db document migration tool",
    "nosql database migration framework",
    "cosmos db version control",
    "azure cosmos db data migration",
    "cosmos db migration c#",
    "cosmos db rollback migration",
    "cosmos db bulk operations",
    "cosmos db indexing policy migration",
    "dotnet cosmos db migration",
  ],
  authors: [{ name: "Cosmigrator Contributors" }],
  openGraph: {
    title: "Cosmigrator — Migration framework for Azure Cosmos DB",
    description:
      "Version-controlled, reversible schema migrations for Azure Cosmos DB. Write C# migration classes, run forward or roll back from the CLI.",
    url: "https://cosmigrator.adellajil.com",
    siteName: "Cosmigrator",
    type: "website",
    locale: "en_US",
  },
  twitter: {
    card: "summary_large_image",
    title: "Cosmigrator — Migration framework for Azure Cosmos DB",
    description:
      "Version-controlled, reversible schema migrations for Azure Cosmos DB. Write C# migration classes, run forward or roll back from the CLI.",
  },
  metadataBase: new URL("https://cosmigrator.adellajil.com"),
  alternates: {
    canonical: "https://cosmigrator.adellajil.com",
  },
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en" className="dark">
      <body className="bg-neutral-950 text-neutral-100 antialiased">
        {children}
      </body>
    </html>
  );
}
