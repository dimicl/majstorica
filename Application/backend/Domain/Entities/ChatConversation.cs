namespace backend.Domain.Entities;

/*DA BI OMOGUCILI RAZGOVOR IZMEDJU KLIJENTA I MAJSTORA
Predstavlja jedan chat kanal između:
-jednog klijenta
-jednog majstora
-vezan za jedan posao

Služi da grupiše poruke i da backend zna:
-ko učestvuje u razgovoru
-za koji posao je vezan razgovor
-da li je razgovor aktivan ili zatvoren

Koristi se u:
-ChatService
-ConversationsController
-SignalR komunikaciji
-repozitorijumu za konverzacije

U praksi:
-klijent pošalje zahtev ili poruku majstoru
-kreira se ChatConversation
-poruke se dalje vezuju za taj conversation
-conversation može da se zatvori ili ponovo otvori
*/
public class ChatConversation
{
    public Guid Id { get; private set; }

    //povezuje konverzaciju sa poslom, ali mi mozemo i da cetujemo bez posla tkd?
    public Guid JobId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid MasterId { get; private set; }

    //da li je konverzacija otvorena ili zatvorena, da se zatvori ukoliko nema posla tkd?
    public bool IsActive { get; private set; }

    protected ChatConversation() { }

    public ChatConversation(Guid jobId, Guid clientId, Guid masterId)
    {
        Id = Guid.NewGuid();
        JobId = jobId;
        ClientId = clientId;
        MasterId = masterId;
        IsActive = true;
    }

    // ---------------- DOMENSKE OPERACIJE ----------------

    public void Close()
    {
        IsActive = false;
    }

    //Ponovo otvara konverzaciju (npr. kad klijent pošalje novi zahtev za posao, ili zeli da ponovo kontaktira majstora)
    public void Reopen()
    {
        IsActive = true;
    }

    //Povezuje konverzaciju (nastalu npr. iz "Napiši poruku") sa poslom, i onda sad nam ovo sluzi da mozes da komuniciras bez posla, a kad se kreira poso da se konverzacija povez sa poso?
    public void AssignJob(Guid jobId)
    {
        JobId = jobId;
    }


    //posto imamo otvaranje i zatvranje chat mozemo da dodamo i domen pravilo da ne mogu da se salju poruke u zatvorenu konverzaciju


    public static ChatConversation Rehydrate(
        Guid id,
        Guid jobId,
        Guid clientId,
        Guid masterId,
        bool isActive)
    {
        return new ChatConversation
        {
            Id = id,
            JobId = jobId,
            ClientId = clientId,
            MasterId = masterId,
            IsActive = isActive
        };
    }
}
