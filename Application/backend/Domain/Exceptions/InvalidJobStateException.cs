namespace backend.Domain.Exceptions;

public class InvalidJobStateException : DomainException
{
    public InvalidJobStateException()
    {
    }

    public InvalidJobStateException(string message)
        : base(message)
    {
    }

    public InvalidJobStateException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}