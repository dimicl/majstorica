using backend.Domain.Exceptions;

namespace backend.Domain.ValueObjects;

public class Address
{
    private Address()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public Address(
        string street,
        string city,
        string? zone = null,
        string? postalCode = null,
        string? country = null)
    {
        SetStreet(street);
        SetCity(city);
        SetZone(zone);
        SetPostalCode(postalCode);
        SetCountry(country);
    }

    public string Street { get; private set; } = string.Empty;

    public string City { get; private set; } = string.Empty;

    public string? Zone { get; private set; }

    public string? PostalCode { get; private set; }

    public string? Country { get; private set; }

    private void SetStreet(string street)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Street is required.");

        var value = street.Trim();

        if (value.Length > 200)
            throw new DomainException("Street is too long.");

        Street = value;
    }

    private void SetCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City is required.");

        var value = city.Trim();

        if (value.Length > 100)
            throw new DomainException("City is too long.");

        City = value;
    }

    private void SetZone(string? zone)
    {
        if (string.IsNullOrWhiteSpace(zone))
        {
            Zone = null;
            return;
        }

        var value = zone.Trim();

        if (value.Length > 100)
            throw new DomainException("Zone is too long.");

        Zone = value;
    }

    private void SetPostalCode(string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
        {
            PostalCode = null;
            return;
        }

        var value = postalCode.Trim();

        if (value.Length > 20)
            throw new DomainException("Postal code is too long.");

        PostalCode = value;
    }

    private void SetCountry(string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            Country = null;
            return;
        }

        var value = country.Trim();

        if (value.Length > 100)
            throw new DomainException("Country is too long.");

        Country = value;
    }

    public override string ToString()
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(Street))
            parts.Add(Street);
        if (!string.IsNullOrWhiteSpace(City))
            parts.Add(City);
        if (!string.IsNullOrWhiteSpace(Zone))
            parts.Add(Zone!);
        if (!string.IsNullOrWhiteSpace(PostalCode))
            parts.Add(PostalCode!);
        if (!string.IsNullOrWhiteSpace(Country))
            parts.Add(Country!);

        return string.Join(", ", parts);
    }
}