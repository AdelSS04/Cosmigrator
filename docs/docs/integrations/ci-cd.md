---
title: CI/CD Pipelines
sidebar_position: 2
---

# CI/CD Integration

Cosmigrator exits with code `0` on success and `1` on failure, making it a natural fit for pipeline steps.

## GitHub Actions

```yaml
name: Deploy
on:
  push:
    branches: [main]

jobs:
  migrate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Build migration project
        run: dotnet build MyService.Migrations -c Release

      - name: Run migrations
        run: dotnet run --project MyService.Migrations -c Release
        env:
          CosmosDb__ConnectionString: ${{ secrets.COSMOS_CONNECTION_STRING }}
          CosmosDb__DatabaseName: ${{ secrets.COSMOS_DATABASE_NAME }}
```

## Azure DevOps

```yaml
steps:
  - task: UseDotNet@2
    inputs:
      version: '10.0.x'

  - script: dotnet build MyService.Migrations -c Release
    displayName: 'Build migrations'

  - script: dotnet run --project MyService.Migrations -c Release
    displayName: 'Apply migrations'
    env:
      CosmosDb__ConnectionString: $(CosmosConnectionString)
      CosmosDb__DatabaseName: $(CosmosDatabaseName)
```

## Docker Compose

Run migrations before your application starts:

```yaml
services:
  migrations:
    build:
      context: .
      dockerfile: MyService.Migrations/Dockerfile
    environment:
      - CosmosDb__ConnectionString=AccountEndpoint=https://...
      - CosmosDb__DatabaseName=MyDatabase

  api:
    build:
      context: .
      dockerfile: MyService.Api/Dockerfile
    depends_on:
      migrations:
        condition: service_completed_successfully
```

## Pipeline patterns

### Gate deployment on migration success

Since Cosmigrator exits with a non-zero code on failure, you can gate subsequent steps:

```yaml
- name: Run migrations
  run: dotnet run --project MyService.Migrations -c Release

- name: Deploy application
  if: success()  # Only runs if migrations succeeded
  run: ./deploy.sh
```

### Check migration status before deploying

```yaml
- name: Check migration status
  run: dotnet run --project MyService.Migrations -c Release -- status
```

### Rollback on deployment failure

```yaml
- name: Rollback migrations
  if: failure()
  run: dotnet run --project MyService.Migrations -c Release -- rollback --steps 1
```
