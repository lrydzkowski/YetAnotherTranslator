# Quickstart Guide: Yet Another Translator

**Feature**: `001-polish-english-translator`
**Date**: 2025-11-02
**Audience**: Developers setting up the project

## Overview

This guide walks you through setting up and running the YetAnotherTranslator CLI tool from scratch. Follow these steps to get the REPL running locally.

## Prerequisites

### Required Software

- **.NET 10 SDK** - Download from [dotnet.microsoft.com](https://dotnet.microsoft.com/)
- **PostgreSQL 16+** - Database for history and caching
  - Windows: Download from [postgresql.org](https://www.postgresql.org/download/windows/)
  - macOS: `brew install postgresql@16`
  - Linux: `sudo apt-get install postgresql-16` (Debian/Ubuntu)
- **Azure CLI** - For authentication to Azure Key Vault
  - Windows: Download from [docs.microsoft.com/cli/azure/install-azure-cli](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
  - macOS: `brew install azure-cli`
  - Linux: Install via package manager

### Required Accounts

- **Azure Account** - For Azure Key Vault (free tier available)
- **Anthropic Account** - For Claude API access ([console.anthropic.com](https://console.anthropic.com/))
- **ElevenLabs Account** - For text-to-speech API ([elevenlabs.io](https://elevenlabs.io/))

## Step 1: Clone and Build

### Clone Repository

```bash
git clone https://github.com/<your-org>/YetAnotherTranslator.git
cd YetAnotherTranslator
```

### Build Solution

```bash
dotnet restore
dotnet build
```

**Expected Output**:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Run Tests

```bash
dotnet test
```

**Expected Output**:

```text
Passed! - Failed: 0, Passed: X, Skipped: 0, Total: X
```

## Step 2: Set Up PostgreSQL

### Create Database

```bash
# Connect to PostgreSQL
psql -U postgres

# Create database and user
CREATE DATABASE yet_another_translator;
CREATE USER translator_user WITH PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE yet_another_translator TO translator_user;
\q
```

### Test Connection

```bash
psql -U translator_user -d yet_another_translator -h localhost

# If successful, you should see:
# yet_another_translator=>
```

### Create Database Schema

```bash
# Run EF Core migrations
dotnet ef database update --project src/YetAnotherTranslator.Infrastructure --startup-project src/YetAnotherTranslator.Cli
```

**Expected Output**:

```text
Applying migration '001_InitialSchema'.
Done.
```

### Verify Tables Created

```bash
psql -U translator_user -d yet_another_translator -h localhost -c "\dt"
```

**Expected Output**:

```text
                  List of relations
 Schema |           Name            | Type  |      Owner
--------+---------------------------+-------+-----------------
 public | history_entries           | table | translator_user
 public | pronunciation_cache       | table | translator_user
 public | text_translation_cache    | table | translator_user
 public | translation_cache         | table | translator_user
```

## Step 3: Set Up Azure Key Vault

### Login to Azure

```bash
az login
```

**Expected Output**: Browser opens for authentication

### Create Key Vault

```bash
# Create resource group (if needed)
az group create --name translator-rg --location eastus

# Create Key Vault
az keyvault create \
  --name yet-another-translator-kv \
  --resource-group translator-rg \
  --location eastus
```

**Note**: Key Vault name must be globally unique. If taken, try adding your initials: `yet-another-translator-<initials>-kv`

### Grant Yourself Access

```bash
# Get your user object ID
az ad signed-in-user show --query id -o tsv

# Grant yourself Key Vault Secrets User role
az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee <your-object-id> \
  --scope /subscriptions/<subscription-id>/resourceGroups/translator-rg/providers/Microsoft.KeyVault/vaults/yet-another-translator-kv
```

### Store API Keys in Key Vault

```bash
# Store Anthropic API key
az keyvault secret set \
  --vault-name yet-another-translator-kv \
  --name anthropic-api-key \
  --value "sk-ant-..."

# Store ElevenLabs API key
az keyvault secret set \
  --vault-name yet-another-translator-kv \
  --name elevenlabs-api-key \
  --value "..."

# Store PostgreSQL connection string
az keyvault secret set \
  --vault-name yet-another-translator-kv \
  --name postgres-connection-string \
  --value "Host=localhost;Database=yet_another_translator;Username=translator_user;Password=your_secure_password"
```

### Verify Secrets Stored

```bash
az keyvault secret list --vault-name yet-another-translator-kv --query "[].name" -o tsv
```

**Expected Output**:

```text
anthropic-api-key
elevenlabs-api-key
postgres-connection-string
```

## Step 4: Configure Application

### Edit Configuration File

The application uses `appsettings.json` for configuration. Edit `src/YetAnotherTranslator.Cli/appsettings.Development.json`:

```json
{
  "KeyVault": {
    "VaultName": "yet-another-translator-kv"
  },
  "LlmProvider": {
    "Provider": "Anthropic",
    "Model": "claude-3-5-sonnet-20241022",
    "ApiKeySecretName": "anthropic-api-key",
    "MaxTokens": 4096,
    "Temperature": 0.3
  },
  "TtsProvider": {
    "Provider": "ElevenLabs",
    "ApiKeySecretName": "elevenlabs-api-key",
    "VoiceId": "21m00Tcm4TlvDq8ikWAM"
  },
  "Database": {
    "ConnectionStringSecretName": "postgres-connection-string"
  }
}
```

**Important**: Replace `yet-another-translator-kv` with your actual Key Vault name!

**How it works**: The application automatically loads secrets from Azure Key Vault using the specified secret names. No manual secret retrieval needed.

### Get ElevenLabs Voice ID (Optional)

```bash
# List available voices (requires ElevenLabs API key)
curl -X GET "https://api.elevenlabs.io/v1/voices" \
  -H "xi-api-key: <your-elevenlabs-api-key>"
```

Choose a voice ID and update `VoiceId` in appsettings.Development.json. Default (`21m00Tcm4TlvDq8ikWAM`) is Rachel (US English female).

### Validate Configuration

Ensure the JSON is valid:

```bash
# Validate JSON syntax
cat src/YetAnotherTranslator.Cli/appsettings.Development.json | jq .
# (Windows PowerShell: Get-Content src/YetAnotherTranslator.Cli/appsettings.Development.json | ConvertFrom-Json)
```

**Expected Output**: JSON printed without errors

## Step 5: Run the Application

### Start the REPL

```bash
dotnet run --project src/YetAnotherTranslator.Cli
```

**Expected Output**:

```text
Yet Another Translator
Type /help for commands, /quit to exit

>
```

### Try First Command

```text
> /t hello
```

**Expected Output**:

```text
╭───┬─────────────┬────────────────┬──────────────┬────────────────────────────────────────╮
│ # │ Translation │ Part of Speech │ Countability │ Example                                │
├───┼─────────────┼────────────────┼──────────────┼────────────────────────────────────────┤
│ 1 │ cześć       │ interjection   │ N/A          │ Cześć, jak się masz?                   │
│ 2 │ witaj       │ interjection   │ N/A          │ Witaj z powrotem!                      │
│ 3 │ dzień dobry │ phrase         │ N/A          │ Dzień dobry, miło pana poznać.         │
╰───┴─────────────┴────────────────┴──────────────┴────────────────────────────────────────╯

(Completed in 2.3s)
```

### Test Pronunciation

```text
> /p hello
```

**Expected Output**:

```text
Playing pronunciation...
▶ "hello"
✓ Playback complete (1.2s)
```

(You should hear audio playback)

### View Help

```text
> /help
```

**Expected Output**: Full command reference

### Exit REPL

```text
> /quit
```

**Expected Output**:

```text
Goodbye!
```

## Troubleshooting

### Error: "Configuration file not found"

**Cause**: appsettings.Development.json not found or invalid

**Fix**:

```bash
# Verify file exists
ls src/YetAnotherTranslator.Cli/appsettings.Development.json

# Verify JSON is valid
cat src/YetAnotherTranslator.Cli/appsettings.Development.json | jq .
```

If missing or invalid, update it following Step 4.

### Error: "Failed to authenticate with Azure Key Vault"

**Cause**: Not logged in to Azure CLI

**Fix**:

```bash
az login
az account show  # Verify logged in
```

### Error: "Secret 'anthropic-api-key' not found in Key Vault"

**Cause**: Secret not stored in Key Vault or wrong name

**Fix**:

```bash
# List secrets in vault
az keyvault secret list --vault-name yet-another-translator-kv --query "[].name" -o tsv

# Verify secret exists, if not, create it:
az keyvault secret set \
  --vault-name yet-another-translator-kv \
  --name anthropic-api-key \
  --value "sk-ant-..."
```

### Error: "Unable to connect to PostgreSQL"

**Cause**: PostgreSQL not running or wrong connection string

**Fix**:

```bash
# Check PostgreSQL is running
# Windows: Check Services app
# macOS: brew services list | grep postgresql
# Linux: systemctl status postgresql

# Test connection manually
psql -U translator_user -d yet_another_translator -h localhost

# Update connection string in Key Vault if needed
az keyvault secret set \
  --vault-name yet-another-translator-kv \
  --name postgres-connection-string \
  --value "Host=localhost;Database=yet_another_translator;Username=translator_user;Password=your_secure_password"
```

### Error: "Anthropic API error: 401 Unauthorized"

**Cause**: Invalid Anthropic API key

**Fix**:

1. Verify API key at [console.anthropic.com](https://console.anthropic.com/)
2. Update in Key Vault:

```bash
az keyvault secret set \
  --vault-name yet-another-translator-kv \
  --name anthropic-api-key \
  --value "sk-ant-..."
```

3. Restart application

### Error: "Audio playback unavailable"

**Cause**: Audio drivers not available or PortAudio not installed

**Fix**:

**Windows**: Should work out of the box
**macOS**: Install PortAudio via Homebrew:

```bash
brew install portaudio
```

**Linux**: Install PortAudio dev package:

```bash
# Debian/Ubuntu
sudo apt-get install portaudio19-dev

# Fedora
sudo dnf install portaudio-devel
```

Then rebuild:

```bash
dotnet clean
dotnet build
```

### Error: "Migration 'XXX_InitialSchema' not found"

**Cause**: EF Core migrations not created

**Fix**:

```bash
# Create initial migration
dotnet ef migrations add InitialSchema \
  --project src/YetAnotherTranslator.Infrastructure \
  --startup-project src/YetAnotherTranslator.Cli

# Apply migration
dotnet ef database update \
  --project src/YetAnotherTranslator.Infrastructure \
  --startup-project src/YetAnotherTranslator.Cli
```

## Next Steps

### Explore Commands

Try all available commands:

```text
> /t kot
> /tt Cześć, jak się masz?
> /r I are going to store
> /p record noun
> /history
```

### Check Cache Performance

Run same command twice to see cache speedup:

```text
> /t hello
(Completed in 2.3s - API call)

> /t hello
(Completed in 0.1s - cached)
```

### Bypass Cache

Use `--no-cache` to force fresh API call:

```text
> /t --no-cache hello
(Completed in 2.4s - fresh API call, cache updated)
```

### Run Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/YetAnotherTranslator.Core.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Publish Application

```bash
# Publish self-contained executable
dotnet publish src/YetAnotherTranslator.Cli \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -o ./publish

# Run published executable
./publish/YetAnotherTranslator.Cli.exe  # Windows
./publish/YetAnotherTranslator.Cli       # macOS/Linux
```

## Development Workflow

### Make Code Changes

1. Edit code in `src/`
2. Run tests: `dotnet test`
3. Build: `dotnet build`
4. Run: `dotnet run --project src/YetAnotherTranslator.Cli`

### Add New Migration

```bash
dotnet ef migrations add <MigrationName> \
  --project src/YetAnotherTranslator.Infrastructure \
  --startup-project src/YetAnotherTranslator.Cli
```

### Update Database Schema

```bash
dotnet ef database update \
  --project src/YetAnotherTranslator.Infrastructure \
  --startup-project src/YetAnotherTranslator.Cli
```

### Reset Database

```bash
# Drop database
dotnet ef database drop --force \
  --project src/YetAnotherTranslator.Infrastructure \
  --startup-project src/YetAnotherTranslator.Cli

# Recreate with migrations
dotnet ef database update \
  --project src/YetAnotherTranslator.Infrastructure \
  --startup-project src/YetAnotherTranslator.Cli
```

## Configuration Reference

### Minimum config.json

```json
{
  "secretManager": {
    "type": "azure-keyvault",
    "keyVaultUrl": "https://<vault-name>.vault.azure.net"
  },
  "llmProvider": {
    "type": "anthropic",
    "secretReference": "anthropic-api-key",
    "model": "claude-3-5-sonnet-20241022",
    "maxTokens": 2048,
    "temperature": 0.3
  },
  "ttsProvider": {
    "type": "elevenlabs",
    "secretReference": "elevenlabs-api-key",
    "voiceId": "21m00Tcm4TlvDq8ikWAM"
  },
  "database": {
    "secretReference": "postgres-connection-string",
    "maxRetryCount": 3,
    "commandTimeout": 30
  }
}
```

### Environment Variable Override (Optional)

You can override config.json values with environment variables:

```bash
# Windows PowerShell
$env:TRANSLATOR_SECRET_MANAGER__KEY_VAULT_URL = "https://my-vault.vault.azure.net"

# macOS/Linux
export TRANSLATOR_SECRET_MANAGER__KEY_VAULT_URL="https://my-vault.vault.azure.net"
```

Variables use double underscore (`__`) for nested properties.

## Performance Benchmarks

| Operation | First Call (API) | Cached | Target |
|-----------|-----------------|--------|---------|
| Word translation | 2-3s | <0.2s | <3s |
| Text translation | 3-5s | <0.2s | <5s |
| Grammar review | 3-5s | <0.2s | <5s |
| Pronunciation | 1-2s | <0.2s | <2s |
| History | N/A | <0.5s | <1s |

## System Requirements

### Minimum

- CPU: 2 cores
- RAM: 2 GB
- Disk: 500 MB (application + database)
- Network: Stable internet connection (for API calls)

### Recommended

- CPU: 4 cores
- RAM: 4 GB
- Disk: 2 GB (for extensive caching)
- Network: High-speed internet (for optimal API performance)

## Security Best Practices

1. **Never commit config.json** to version control
2. **Use Azure RBAC** for Key Vault access (not access policies)
3. **Rotate API keys** regularly
4. **Use strong passwords** for PostgreSQL
5. **Enable Azure Key Vault soft delete** for secret recovery
6. **Monitor API usage** to detect anomalies
7. **Keep .NET SDK updated** for security patches

## Support

### Documentation

- Full spec: `specs/001-polish-english-translator/spec.md`
- Data model: `specs/001-polish-english-translator/data-model.md`
- Commands: `specs/001-polish-english-translator/contracts/command-interface.md`

### Common Issues

- Check troubleshooting section above
- Review application logs (stderr output)
- Verify all prerequisites installed
- Test each component individually (database, Key Vault, APIs)

### Further Help

- Open issue on GitHub repository
- Check Azure Key Vault access logs
- Review EF Core migration history
- Test API keys directly via curl/Postman

## Summary

You should now have:
- ✓ .NET 10 SDK installed
- ✓ PostgreSQL database created and migrated
- ✓ Azure Key Vault configured with secrets
- ✓ Application configuration file in place
- ✓ REPL running and responsive to commands
- ✓ All APIs (Anthropic, ElevenLabs) accessible
- ✓ Caching working for improved performance

Ready to translate! Type `/help` in the REPL for full command reference.
