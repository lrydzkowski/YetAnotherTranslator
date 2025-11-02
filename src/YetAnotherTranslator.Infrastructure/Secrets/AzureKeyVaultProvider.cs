using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Collections.Concurrent;
using YetAnotherTranslator.Core.Interfaces;

namespace YetAnotherTranslator.Infrastructure.Secrets;

public class AzureKeyVaultProvider : ISecretsProvider
{
    private readonly SecretClient _client;
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public AzureKeyVaultProvider(string keyVaultUrl)
    {
        _client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
    }

    public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(secretName, out var cachedValue))
        {
            return cachedValue;
        }

        var secret = await _client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
        _cache[secretName] = secret.Value.Value;
        return secret.Value.Value;
    }
}
