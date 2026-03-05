namespace backend.Shared.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Nemate pristup ovom resursu.") 
        : base(message)
    {
    }
}
