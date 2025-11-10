using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Core.Interfaces;

namespace YetAnotherTranslator.Infrastructure.Secrets;

public class AzureKeyVaultSecretsProvider : ISecretsProvider
{
    private readonly SecretClient _client;

    public AzureKeyVaultSecretsProvider(string keyVaultUrl)
    {
        if (string.IsNullOrWhiteSpace(keyVaultUrl))
        {
            throw new ConfigurationException("Key Vault URL cannot be empty");
        }

        if (!Uri.TryCreate(keyVaultUrl, UriKind.Absolute, out Uri? uri) || uri.Scheme != "https")
        {
            throw new ConfigurationException($"Invalid Key Vault URL: {keyVaultUrl}. Must be a valid HTTPS URL");
        }

        try
        {
            _client = new SecretClient(uri, new DefaultAzureCredential());
        }
        catch (Exception ex)
        {
            throw new ConfigurationException($"Failed to initialize Azure Key Vault client: {ex.Message}", ex);
        }
    }

    public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secretName))
        {
            throw new ArgumentException("Secret name cannot be empty", nameof(secretName));
        }

        try
        {
            Response<KeyVaultSecret> response = await _client.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            return response.Value.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new ExternalServiceException(
                "AzureKeyVault",
                $"Secret '{secretName}' not found in Key Vault"
            );
        }
        catch (RequestFailedException ex) when (ex.Status == 403)
        {
            throw new ExternalServiceException(
                "AzureKeyVault",
                "Access denied to Key Vault. Please check your permissions and ensure you are authenticated (az login)"
            );
        }
        catch (RequestFailedException ex)
        {
            throw new ExternalServiceException(
                "AzureKeyVault",
                $"Failed to retrieve secret from Key Vault: {ex.Message}",
                ex
            );
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new ExternalServiceException(
                "AzureKeyVault",
                "Connection to Azure Key Vault timed out. Please check your network connection and Key Vault URL",
                ex
            );
        }
        catch (Exception ex)
        {
            throw new ExternalServiceException(
                "AzureKeyVault",
                $"Unexpected error retrieving secret from Key Vault: {ex.Message}",
                ex
            );
        }
    }
}
