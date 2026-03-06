using backend.Domain.Enums;

namespace backend.Domain.Entities;

/*DA PRATI TRENUTNO STANJE KORISNIKA, NE TRAJNE PODATKE
Predstavlja aktivnu sesiju korisnika:
-ko je korisnik
-koja mu je role
-koju SignalR konekciju ima
-koji posao trenutno gleda
-koji chat trenutno gleda
-kada je poslednji put bio aktivan

Služi za:
-realtime prisustvo korisnika
-online/offline status
-write ownership nad dokumentom/poslom
-praćenje otvorenog chata ili posla
-SignalR vezu sa konkretnim korisnikom

Koristi se u:
-SessionService
-RedisSessionRepository
-DocumentHub
-ChatService
-SignalR logici

U praksi:
-korisnik se poveže → kreira se ili update-uje UserSession
-uđe u posao → CurrentJobId
-uđe u chat → CurrentConversationId
-backend zna gde je korisnik i kome da šalje realtime event
*/
public class UserSession
{
    public string Id { get; internal set; } = default!;
    public Guid UserId { get; internal set; }
    public UserRole Role { get; internal set; }
    public Guid? CurrentJobId { get; internal set; }
    public Guid? CurrentConversationId { get; internal set; }
    public string ConnectionId { get; internal set; } = default!;
    public DateTime LastSeen { get; internal set; } = DateTime.UtcNow;

    protected UserSession() { }

    //korisnik se uloguje, kreira se userSession i cuva u redis
    public UserSession(string id, Guid userId, UserRole role, string connectionId)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        UserId = userId;
        Role = role;
        ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
        LastSeen = DateTime.UtcNow;
    }

    //isto kao rehydrate za citanje iz baze
    public static UserSession FromPersistence(
        string id,
        Guid userId,
        UserRole role,
        Guid? currentJobId,
        Guid? currentConversationId,
        string connectionId,
        DateTime lastSeen)
    {
        return new UserSession
        {
            Id = id,
            UserId = userId,
            Role = role,
            CurrentJobId = currentJobId,
            CurrentConversationId = currentConversationId,
            ConnectionId = connectionId,
            LastSeen = lastSeen
        };
    }


    // ---------------- DOMENSKE OPERACIJE ----------------
    public void SetCurrentJob(Guid? jobId)
    {
        CurrentJobId = jobId;
        LastSeen = DateTime.UtcNow;
    }

    public void SetCurrentConversation(Guid? conversationId)
    {
        CurrentConversationId = conversationId;
        LastSeen = DateTime.UtcNow;
    }

    public void Touch()
    {
        LastSeen = DateTime.UtcNow;
    }
}
