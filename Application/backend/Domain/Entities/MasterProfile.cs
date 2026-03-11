using backend.Domain.Exceptions;
using backend.Domain.ValueObjects;

namespace backend.Domain.Entities;

public class MasterProfile
{
    private readonly List<string> _serviceCategories = new();
    private readonly List<string> _serviceZones = new();

    private MasterProfile()
    {
        // Potrebno za serializer / mapper / Mongo
    }

    public MasterProfile(
        string headline,
        string? description,
        int yearsOfExperience,
        Money hourlyRate,
        bool isAvailable,
        IEnumerable<string>? serviceCategories = null,
        IEnumerable<string>? serviceZones = null)
    {
        SetHeadline(headline);
        SetDescription(description);
        SetYearsOfExperience(yearsOfExperience);
        SetHourlyRate(hourlyRate);

        IsAvailable = isAvailable;
        AverageRating = null;
        TotalJobsCompleted = 0;
        TotalReviews = 0;

        if (serviceCategories is not null)
            SetServiceCategories(serviceCategories);

        if (serviceZones is not null)
            SetServiceZones(serviceZones);
    }

    public string Headline { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int YearsOfExperience { get; private set; }

    public Money HourlyRate { get; private set; } = null!;

    public bool IsAvailable { get; private set; }

    public Rating? AverageRating { get; private set; }

    public int TotalJobsCompleted { get; private set; }

    public int TotalReviews { get; private set; }

    public IReadOnlyCollection<string> ServiceCategories => _serviceCategories.AsReadOnly();

    public IReadOnlyCollection<string> ServiceZones => _serviceZones.AsReadOnly();

    public void UpdateBasicInfo(
        string headline,
        string? description,
        int yearsOfExperience,
        Money hourlyRate,
        bool isAvailable)
    {
        SetHeadline(headline);
        SetDescription(description);
        SetYearsOfExperience(yearsOfExperience);
        SetHourlyRate(hourlyRate);
        IsAvailable = isAvailable;
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
    }

    public void SetHourlyRate(Money hourlyRate)
    {
        HourlyRate = hourlyRate ?? throw new DomainException("Hourly rate is required.");
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
            throw new DomainException("Master must have at least one service category.");
    }

    public void AddServiceCategory(string category)
    {
        var normalizedCategory = NormalizeListValue(category, "Service category");

        if (_serviceCategories.Any(x => x.Equals(normalizedCategory, StringComparison.OrdinalIgnoreCase)))
            return;

        _serviceCategories.Add(normalizedCategory);
    }

    public void RemoveServiceCategory(string category)
    {
        var existingCategory = _serviceCategories.FirstOrDefault(x =>
            x.Equals(category?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (existingCategory is null)
            return;

        if (_serviceCategories.Count == 1)
            throw new DomainException("Master must have at least one service category.");

        _serviceCategories.Remove(existingCategory);
    }

    public void ReplaceServiceCategories(IEnumerable<string> categories)
    {
        SetServiceCategories(categories);
    }

    public void SetServiceZones(IEnumerable<string> zones)
    {
        if (zones is null)
            throw new DomainException("Service zones collection cannot be null.");

        _serviceZones.Clear();

        foreach (var zone in zones)
        {
            var normalizedZone = NormalizeListValue(zone, "Service zone");

            if (_serviceZones.Any(x => x.Equals(normalizedZone, StringComparison.OrdinalIgnoreCase)))
                continue;

            _serviceZones.Add(normalizedZone);
        }

        if (_serviceZones.Count == 0)
            throw new DomainException("Master must have at least one service zone.");
    }

    public void AddServiceZone(string zone)
    {
        var normalizedZone = NormalizeListValue(zone, "Service zone");

        if (_serviceZones.Any(x => x.Equals(normalizedZone, StringComparison.OrdinalIgnoreCase)))
            return;

        _serviceZones.Add(normalizedZone);
    }

    public void RemoveServiceZone(string zone)
    {
        var existingZone = _serviceZones.FirstOrDefault(x =>
            x.Equals(zone?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (existingZone is null)
            return;

        if (_serviceZones.Count == 1)
            throw new DomainException("Master must have at least one service zone.");

        _serviceZones.Remove(existingZone);
    }

    public void ReplaceServiceZones(IEnumerable<string> zones)
    {
        SetServiceZones(zones);
    }

    public void IncrementCompletedJobs()
    {
        TotalJobsCompleted++;
    }

    public void UpdateRating(Rating averageRating, int totalReviews)
    {
        AverageRating = averageRating ?? throw new DomainException("Rating cannot be null.");

        if (totalReviews < 0)
            throw new DomainException("Total reviews cannot be negative.");

        TotalReviews = totalReviews;
    }

    private void SetHeadline(string headline)
    {
        if (string.IsNullOrWhiteSpace(headline))
            throw new DomainException("Headline is required.");

        var value = headline.Trim();

        if (value.Length > 120)
            throw new DomainException("Headline cannot be longer than 120 characters.");

        Headline = value;
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
            throw new DomainException("Description cannot be longer than 2000 characters.");

        Description = value;
    }

    private void SetYearsOfExperience(int yearsOfExperience)
    {
        if (yearsOfExperience < 0)
            throw new DomainException("Years of experience cannot be negative.");

        if (yearsOfExperience > 80)
            throw new DomainException("Years of experience is not valid.");

        YearsOfExperience = yearsOfExperience;
    }

    private static string NormalizeListValue(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{fieldName} is required.");

        return value.Trim();
    }
}