using backend.Domain.Enums;

namespace backend.Domain.Entities;


/*ODVAJA KORISNICKI NALOG OD ULOGE MAJSTORA KOJI IZVRSAVA POSLOVE
Predstavlja profesionalni profil majstora:
-biografija
-kategorija
-iskustvo
-ocena
-recenzije

Služi da backend zna:
-čime se majstor bavi
-koliko iskustva ima
-kakvu reputaciju ima
-u kojoj kategoriji može da prima poslove

Koristi se u:
-pretrazi majstora
-slanju zahteva majstorima
-prihvatanju poslova
-sistemu recenzija i ocenjivanja
-graf bazi za veze sa poslovima i klijentima

U praksi:
-napravi se User
-napravi se Master
-klijent šalje zahtev majstoru
-majstor prihvati posao
-posle završetka dobije review i rating
*/
public class Master
{
    public Guid Id { get; internal set; }
    public Guid UserId { get; internal set; }

    public string? Bio { get; internal set; }

    public MasterCategory? Category { get; internal set; }

    public decimal? Rating { get; internal set; }

    public IList<Review> Reviews { get; internal set; } = new List<Review>();

    public int? YearsExperience { get; internal set; }

    public DateTime CreatedAt { get; internal set; }
    public DateTime? UpdatedAt { get; internal set; }

    protected Master() { }

    public Master(Guid userId, string? bio = null, MasterCategory? category = null, int? yearsExperience = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Bio = bio;
        Category = category;
        YearsExperience = yearsExperience;
        CreatedAt = DateTime.UtcNow;
    }

    public static Master Rehydrate(
        Guid id,
        Guid userId,
        string? bio,
        MasterCategory? category,
        decimal? rating,
        int? yearsExperience,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        return new Master
        {
            Id = id,
            UserId = userId,
            Bio = bio,
            Category = category,
            Rating = rating,
            YearsExperience = yearsExperience,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    // ---------------- DOMENSKE OPERACIJE ----------------
    //ove peracije nigde jos ne koristimo mislim da vtreba da se doda MasterService
    public void UpdateBio(string? bio)
    {
        Bio = bio;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCategory(MasterCategory? category)
    {
        Category = category;
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
