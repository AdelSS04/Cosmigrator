# Security Policy

## Supported Versions

We release patches for security vulnerabilities. Which versions are eligible for receiving such patches depends on the CVSS v3.0 Rating:

| Version | Supported          |
| ------- | ------------------ |
| 1.x.x   | :white_check_mark: |

## Reporting a Vulnerability

**Please do not report security vulnerabilities through public GitHub issues.**

Instead, please report them via email to the maintainers. You should receive a response within 48 hours. If for some reason you do not, please follow up to ensure we received your original message.

Please include the following information (as much as you can provide) to help us better understand the nature and scope of the possible issue:

* Type of issue (e.g. buffer overflow, SQL injection, cross-site scripting, etc.)
* Full paths of source file(s) related to the manifestation of the issue
* The location of the affected source code (tag/branch/commit or direct URL)
* Any special configuration required to reproduce the issue
* Step-by-step instructions to reproduce the issue
* Proof-of-concept or exploit code (if possible)
* Impact of the issue, including how an attacker might exploit it

This information will help us triage your report more quickly.

## Preferred Languages

We prefer all communications to be in English.

## Security Update Policy

When we receive a security bug report, we will:

1. Confirm the problem and determine the affected versions
2. Audit code to find any similar problems
3. Prepare fixes for all supported versions
4. Release new versions as soon as possible

## Security-Related Configuration

### Connection String Security

**Never commit connection strings or credentials to source control.** Always use:

- Azure Key Vault
- User Secrets (for local development)
- Environment variables
- Azure Managed Identity (recommended for production)

Example secure configuration in `appsettings.json`:

```json
{
  "CosmosDb": {
    "Endpoint": "https://your-account.documents.azure.com:443/",
    "Key": "", // Leave empty, override with env var or Key Vault
    "DatabaseName": "YourDatabase"
  }
}
```

Override with environment variables:
```bash
export CosmosDb__Key="YOUR_ACTUAL_KEY"
```

### Least Privilege Access

When running migrations:

- Use a dedicated service principal or managed identity
- Grant only the necessary permissions:
  - Read/Write access to target containers
  - Read/Write access to `__MigrationHistory` container
  - Container management permissions (only if migrations create/delete containers)

### Network Security

- Use private endpoints for Cosmos DB when possible
- Restrict IP addresses in Cosmos DB firewall rules
- Use Azure Virtual Networks for production environments

## Known Security Limitations

1. **No Encryption at Rest Configuration**: This library does not configure encryption at rest. Ensure your Cosmos DB account has encryption enabled at the Azure level.

2. **Logging Sensitive Data**: Be cautious about logging document contents. The library logs migration progress but does not log document data by default. Ensure your custom migrations don't log sensitive information.

3. **Migration Rollback**: Rollback operations may expose data if not implemented carefully. Always test rollback procedures in non-production environments.

## Security Best Practices

1. **Audit Migrations**: Review all migration code before deployment
2. **Test in Isolation**: Test migrations in dedicated dev/staging environments
3. **Backup Data**: Always backup data before running migrations in production
4. **Monitor Execution**: Monitor migration execution logs for anomalies
5. **Rotate Credentials**: Regularly rotate Cosmos DB access keys
6. **Use RBAC**: Prefer Azure RBAC over connection string authentication

## Disclosure Policy

When we release a security fix, we will:

1. Credit the reporter (unless they prefer to remain anonymous)
2. Publish a security advisory on GitHub
3. Update this SECURITY.md with any relevant information
4. Release updated NuGet packages

Thank you for helping keep Cosmigrator and its users safe!
