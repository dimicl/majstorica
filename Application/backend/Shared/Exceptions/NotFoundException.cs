using backend.Domain.Exceptions;

namespace backend.Shared.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message)
    {
    }
}