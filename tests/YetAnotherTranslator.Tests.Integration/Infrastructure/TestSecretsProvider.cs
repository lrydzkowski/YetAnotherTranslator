using System.Net;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Core.Interfaces;

namespace YetAnotherTranslator.Tests.Integration.Infrastructure;

public class TestSecretsProvider : ISecretsProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public TestSecretsProvider(string baseUrl)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_baseUrl}/secrets/{secretName}",
                cancellationToken
            );

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new ExternalServiceException(
                    "AzureKeyVault",
                    $"Secret '{secretName}' not found in Key Vault"
                );
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                throw new ExternalServiceException(
                    "AzureKeyVault",
                    "Access denied to Key Vault. Please check your permissions and ensure you are authenticated (az login)"
                );
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            throw new ExternalServiceException(
                "AzureKeyVault",
                "Connection to Azure Key Vault timed out. Please check your network connection and Key Vault URL",
                ex
            );
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException(
                "AzureKeyVault",
                $"Failed to connect to Azure Key Vault: {ex.Message}",
                ex
            );
        }
        catch (ExternalServiceException)
        {
            throw;
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
