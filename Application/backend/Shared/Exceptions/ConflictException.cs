using backend.Domain.Exceptions;

namespace backend.Shared.Exceptions;

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message)
    {
    }
}