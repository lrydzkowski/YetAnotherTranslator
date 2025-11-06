using System.Text.Json;
using FluentValidation;
using YetAnotherTranslator.Core.Exceptions;
using YetAnotherTranslator.Infrastructure.Configuration.Validators;

namespace YetAnotherTranslator.Infrastructure.Configuration;

public class ConfigurationLoader
{
    private readonly IValidator<ApplicationConfiguration> _validator;
    private readonly string? _customConfigPath;

    public ConfigurationLoader(string? customConfigPath = null)
    {
        _validator = new ApplicationConfigurationValidator();
        _customConfigPath = customConfigPath;
    }

    public async Task<ApplicationConfiguration> LoadConfigurationAsync(CancellationToken cancellationToken = default)
    {
        string configPath = _customConfigPath ?? GetConfigurationPath();

        if (!File.Exists(configPath))
        {
            throw new ConfigurationException($"Configuration file not found at: {configPath}");
        }

        string jsonContent;
        try
        {
            jsonContent = await File.ReadAllTextAsync(configPath, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new ConfigurationException($"Failed to read configuration file at: {configPath}", ex);
        }

        ApplicationConfiguration? config;
        try
        {
            config = JsonSerializer.Deserialize<ApplicationConfiguration>(
                jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
        catch (JsonException ex)
        {
            throw new ConfigurationException(
                $"Malformed JSON in configuration file at: {configPath}. Line: {ex.LineNumber}, Position: {ex.BytePositionInLine}",
                ex
            );
        }

        if (config == null)
        {
            throw new ConfigurationException($"Configuration file at {configPath} is empty or invalid");
        }

        var validationResult = await _validator.ValidateAsync(config, cancellationToken);
        if (!validationResult.IsValid)
        {
            string errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ConfigurationException($"Configuration validation failed: {errors}");
        }

        return config;
    }

    private static string GetConfigurationPath()
    {
        string configDirectory = GetConfigurationDirectory();
        return Path.Combine(configDirectory, "config.json");
    }

    private static string GetConfigurationDirectory()
    {
        string? appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (string.IsNullOrWhiteSpace(appDataPath))
        {
            throw new ConfigurationException("Unable to determine user configuration directory");
        }

        return Path.Combine(appDataPath, "translator");
    }
}
