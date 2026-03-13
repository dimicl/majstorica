using backend.Domain.Exceptions;

namespace backend.Shared.Exceptions;

public class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message)
    {
    }
}