using backend.Domain.Exceptions;

namespace backend.Domain.ValueObjects;

public class Rating
{
    private Rating()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public Rating(decimal value)
    {
        if (value < 1 || value > 5)
            throw new DomainException("Rating must be between 1 and 5.");

        Value = value;
    }

    public decimal Value { get; private set; }
}