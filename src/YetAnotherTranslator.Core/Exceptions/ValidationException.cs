namespace YetAnotherTranslator.Core.Exceptions;

public class ValidationException : TranslatorException
{
    public ValidationException()
    {
    }

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
