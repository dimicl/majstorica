namespace backend.Domain.Entities;

/*NAPRAVLJEN DA SISTEM IMA MEHANIZAM OCENJIVANJA MAJSTORA, PREDSTAVLJA OCENU I KOMENTAR KOJU KLIJENT DAJE MAJSTORU
Služi za:
-reputacioni sistem
-prikaz prosečne ocene majstora
-komentare i istoriju kvaliteta rada

Koristi se u:
-ReviewService ili sličnom servisu za kreiranje recenzije
-Master entitetu za računanje/proslek ratinga
-repozitorijumu za review podatke
-frontend prikazu profila majstora

U praksi:
-posao se završi
-klijent ostavi ocenu 1–5 i komentar
-review se sačuva
-backend preračuna novi prosečni rating majstora
*/
public class Review
{
    public Guid Id { get; internal set; }

    public Guid JobId { get; internal set; }
    public Guid ClientId { get; internal set; }
    public Guid MasterId { get; internal set; }

    public int Rating { get; internal set; }

    public string? Comment { get; internal set; }

    public DateTime CreatedAt { get; internal set; }

    protected Review() { }

    public static Review Rehydrate(Guid id, Guid jobId, Guid clientId, Guid masterId, int rating, string? comment, DateTime createdAt)
    {
        return new Review
        {
            Id = id,
            JobId = jobId,
            ClientId = clientId,
            MasterId = masterId,
            Rating = rating,
            Comment = comment,
            CreatedAt = createdAt
        };
    }

    public Review(Guid jobId, Guid clientId, Guid masterId, int rating, string? comment = null)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Ocena mora biti između 1 i 5.");

        Id = Guid.NewGuid();
        JobId = jobId;
        ClientId = clientId;
        MasterId = masterId;
        Rating = rating;
        Comment = comment;
        CreatedAt = DateTime.UtcNow;
    }

    //mozemo da dodamo domenske operacije za updateRating i updateComment, da ne bude da review moze samo da se kreira i obrise
}
