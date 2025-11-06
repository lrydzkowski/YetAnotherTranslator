namespace YetAnotherTranslator.Core.Exceptions;

public class ExternalServiceException : TranslatorException
{
    public ExternalServiceException(string serviceName)
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message) : base(message)
    {
        ServiceName = serviceName;
    }

    public ExternalServiceException(string serviceName, string message, Exception innerException)
        : base(message, innerException)
    {
        ServiceName = serviceName;
    }

    public string ServiceName { get; }
}
