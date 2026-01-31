namespace backend.Shared.Exceptions;

public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() 
        : base("Pogrešno korisničko ime ili lozinka.")
    {
    }
}
