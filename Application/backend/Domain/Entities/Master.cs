namespace backend.Domain.Entities;
public class Master
{
    public Guid UserId { get; internal set; }

    public string? Bio { get; internal set; }

    public IReadOnlyList<string> Categories { get; internal set; } = new List<string>();

    public decimal? Rating { get; internal set; }

    public IList<Review> Reviews { get; internal set; } = new List<Review>();

    public int? YearsExperience { get; internal set; }

    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }

    protected Master() { }

    public Master(Guid userId, string? bio = null, IEnumerable<string>? categories = null, int? yearsExperience = null)
    {
        UserId = userId;
        Bio = bio;
        Categories = (categories?.ToList() ?? new List<string>()).AsReadOnly();
        YearsExperience = yearsExperience;
        CreatedAt = DateTime.UtcNow;
    }

    public static Master Rehydrate(
        Guid userId,
        string? bio,
        IReadOnlyList<string> categories,
        decimal? rating,
        int? yearsExperience,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        return new Master
        {
            UserId = userId,
            Bio = bio,
            Categories = categories,
            Rating = rating,
            YearsExperience = yearsExperience,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public void UpdateBio(string? bio)
    {
        Bio = bio;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCategories(IEnumerable<string> categories)
    {
        Categories = (categories?.ToList() ?? new List<string>()).AsReadOnly();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateYearsExperience(int? years)
    {
        YearsExperience = years;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRating(decimal? averageRating)
    {
        if (averageRating.HasValue && (averageRating.Value < 1 || averageRating.Value > 5))
            throw new ArgumentOutOfRangeException(nameof(averageRating), "Ocena mora biti između 1 i 5.");
        Rating = averageRating;
        UpdatedAt = DateTime.UtcNow;
    }
           
}
