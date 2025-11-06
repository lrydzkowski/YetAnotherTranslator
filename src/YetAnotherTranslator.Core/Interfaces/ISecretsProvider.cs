namespace YetAnotherTranslator.Core.Interfaces;

public interface ISecretsProvider
{
    Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);
}
