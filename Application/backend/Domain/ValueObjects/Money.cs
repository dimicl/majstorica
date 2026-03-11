using backend.Domain.Exceptions;

namespace backend.Domain.ValueObjects;

public class Money
{
    private Money()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new DomainException("Money amount cannot be negative.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required.");

        var normalizedCurrency = currency.Trim().ToUpperInvariant();

        if (normalizedCurrency.Length > 10)
            throw new DomainException("Currency is not valid.");

        Amount = amount;
        Currency = normalizedCurrency;
    }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = string.Empty;
}