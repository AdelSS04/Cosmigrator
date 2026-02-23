---
title: Kubernetes
sidebar_position: 1
---

# Kubernetes Integration

Run Cosmigrator as an init container so migrations complete before your application starts.

## Init container pattern

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: myservice
spec:
  template:
    spec:
      initContainers:
        - name: migrations
          image: myregistry/myservice-migrations:latest
          command: ["dotnet", "MyService.Migrations.dll"]
          env:
            - name: CosmosDb__ConnectionString
              valueFrom:
                secretKeyRef:
                  name: cosmos-secrets
                  key: connection-string
            - name: CosmosDb__DatabaseName
              value: "MyDatabase"
      containers:
        - name: myservice
          image: myregistry/myservice:latest
```

## How it works

- The init container runs `dotnet MyService.Migrations.dll` which calls `MigrationHost.RunAsync`
- Cosmigrator exits with code `0` on success, `1` on failure
- Kubernetes blocks the main container until the init container exits successfully
- If a migration fails, the pod never starts — the deployment rolls back automatically

## Dockerfile for the migration project

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish MyService.Migrations -c Release -o /app

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "MyService.Migrations.dll"]
```

## Configuration via environment variables

Cosmigrator uses the standard .NET configuration system. Environment variables with `__` separators map to nested JSON keys:

| Environment Variable | Config Key |
|---------------------|-----------|
| `CosmosDb__ConnectionString` | `CosmosDb:ConnectionString` |
| `CosmosDb__DatabaseName` | `CosmosDb:DatabaseName` |

## Rollback in Kubernetes

To roll back from a Kubernetes context, override the command:

```yaml
command: ["dotnet", "MyService.Migrations.dll", "rollback", "--steps", "1"]
```

Or run it as a one-off job:

```bash
kubectl run migration-rollback \
  --image=myregistry/myservice-migrations:latest \
  --restart=Never \
  --env="CosmosDb__ConnectionString=..." \
  --env="CosmosDb__DatabaseName=MyDatabase" \
  -- dotnet MyService.Migrations.dll rollback --steps 1
```
