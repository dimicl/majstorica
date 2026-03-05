namespace backend.Shared.Exceptions;

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string field, string value) 
        : base($"{field} '{value}' je već zauzet.")
    {
        Field = field;
        Value = value;
    }

    public string Field { get; }
    public string Value { get; }
}
