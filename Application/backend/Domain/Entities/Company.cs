using backend.Domain.Exceptions;
using backend.Domain.ValueObjects;

namespace backend.Domain.Entities;

public class Company
{
    private readonly List<string> _serviceCategories = new();

    private Company()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public Company(
        Guid id,
        string name,
        Guid ownerUserId,
        string? description,
        string phoneNumber,
        string email,
        Address? address,
        GeoLocation? geoLocation,
        IEnumerable<string>? serviceCategories,
        DateTime createdAtUtc)
    {
        if (id == Guid.Empty)
            throw new DomainException("Company id cannot be empty.");

        if (ownerUserId == Guid.Empty)
            throw new DomainException("Owner user id cannot be empty.");

        Id = id;
        OwnerUserId = ownerUserId;

        SetName(name);
        SetDescription(description);
        SetPhoneNumber(phoneNumber);
        SetEmail(email);

        Address = address;
        GeoLocation = geoLocation;

        IsActive = true;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = createdAtUtc;

        if (serviceCategories is not null)
            SetServiceCategories(serviceCategories);
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public string PhoneNumber { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public Address? Address { get; private set; }

    public GeoLocation? GeoLocation { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<string> ServiceCategories => _serviceCategories.AsReadOnly();

    public void UpdateBasicInfo(
        string name,
        string? description,
        string phoneNumber,
        string email)
    {
        EnsureActive();

        SetName(name);
        SetDescription(description);
        SetPhoneNumber(phoneNumber);
        SetEmail(email);

        Touch();
    }

    public void SetAddress(Address? address)
    {
        EnsureActive();
        Address = address;
        Touch();
    }

    public void SetGeoLocation(GeoLocation? geoLocation)
    {
        EnsureActive();
        GeoLocation = geoLocation;
        Touch();
    }

    public void SetServiceCategories(IEnumerable<string> categories)
    {
        if (categories is null)
            throw new DomainException("Service categories collection cannot be null.");

        _serviceCategories.Clear();

        foreach (var category in categories)
        {
            var normalizedCategory = NormalizeListValue(category, "Service category");

            if (_serviceCategories.Any(x => x.Equals(normalizedCategory, StringComparison.OrdinalIgnoreCase)))
                continue;

            _serviceCategories.Add(normalizedCategory);
        }

        if (_serviceCategories.Count == 0)
            throw new DomainException("Company must have at least one service category.");
    }

    public void AddServiceCategory(string category)
    {
        EnsureActive();

        var normalizedCategory = NormalizeListValue(category, "Service category");

        if (_serviceCategories.Any(x => x.Equals(normalizedCategory, StringComparison.OrdinalIgnoreCase)))
            return;

        _serviceCategories.Add(normalizedCategory);
        Touch();
    }

    public void RemoveServiceCategory(string category)
    {
        EnsureActive();

        var existingCategory = _serviceCategories.FirstOrDefault(x =>
            x.Equals(category?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (existingCategory is null)
            return;

        if (_serviceCategories.Count == 1)
            throw new DomainException("Company must have at least one service category.");

        _serviceCategories.Remove(existingCategory);
        Touch();
    }

    public void ReplaceServiceCategories(IEnumerable<string> categories)
    {
        EnsureActive();
        SetServiceCategories(categories);
        Touch();
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        Touch();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Company name is required.");

        var value = name.Trim();

        if (value.Length > 150)
            throw new DomainException("Company name cannot be longer than 150 characters.");

        Name = value;
    }

    private void SetDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            Description = null;
            return;
        }

        var value = description.Trim();

        if (value.Length > 2000)
            throw new DomainException("Company description cannot be longer than 2000 characters.");

        Description = value;
    }

    private void SetPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new DomainException("Company phone number is required.");

        PhoneNumber = phoneNumber.Trim();
    }

    private void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Company email is required.");

        Email = email.Trim().ToLowerInvariant();
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new DomainException("Inactive company cannot be modified.");
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }

    private static string NormalizeListValue(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{fieldName} is required.");

        return value.Trim();
    }
}