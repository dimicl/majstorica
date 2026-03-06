namespace backend.Domain.Entities;


/*ODVAJA KORISNICKI NALOG OD ULOGE KLIJENTA KOJI TRAZI USLUGE I POSTAVLJA POSLOVE
Služi da modeluje klijenta kao posebnog domen učesnika sistema.
To omogućava da kasnije za klijenta dodamo posebne stvari kao što su:
-istorija poslova
-omiljeni majstori
-adrese
-način plaćanja

Koristi se u:
-registraciji klijenta
-JobService kada klijent kreira posao
-repozitorijumu za čuvanje klijenata

U praksi:
-napravi se User
-zatim se napravi Client povezan preko UserId
-kad klijent koristi sistem, backend zna da je to baš poslovni akter “client”
*/
public class Client
{
    public Guid Id { get; internal set; }
    public Guid UserId { get; internal set; }

    public DateTime CreatedAt { get; internal set; }

    public DateTime? UpdatedAt { get; internal set; }

    protected Client() { }

    //ovde se ne koristi nigde phone i delivery address i to imamo i u user, a sto bi imali u user delivery address ovo treba da se sredi
    public Client(Guid userId, string? phone = null, string? deliveryAddress = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }


    public static Client Rehydrate(
        Guid id,
        Guid userId,
        DateTime createdAt,
        DateTime? updatedAt
    )
    {
        return new Client
        {
            Id = id,
            UserId = userId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    //nema domenske operacije
}
