namespace YetAnotherTranslator.Core.Common.Exceptions;

public class ConfigurationException : TranslatorException
{
    public ConfigurationException()
    {
    }

    public ConfigurationException(string message) : base(message)
    {
    }

    public ConfigurationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
