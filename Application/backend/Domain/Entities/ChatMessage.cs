namespace backend.Domain.Entities;

/*NAPRAVLJEN DA PREDSTAVLJA JEDNU PORUKU
Predstavlja jednu chat poruku:
-ko ju je poslao
-u kojoj konverzaciji
-za koji posao
-kada je poslata
-da li je sistemska ili korisnička

Služi za:
-realtime razmenu poruka
-istoriju chata
-sistemske poruke poput:
    -posao prihvaćen
    -posao započet
    -posao završen

Koristi se u:
-ChatService
-SignalR hub-u
-repozitorijumu za poruke
-frontend chat prikazu

U praksi:
-korisnik pošalje poruku
-backend napravi ChatMessage
-sačuva je
-prosledi je drugoj strani preko SignalR-a
*/
public class ChatMessage
{
    public string Id { get; internal set; } = Guid.NewGuid().ToString();
    public Guid ConversationId { get; internal set; }
    public Guid JobId { get; internal set; }
    public Guid SenderId { get; internal set; }
    public string Content { get; internal set; } = default!;
    public DateTime SentAt { get; internal set; } = DateTime.UtcNow;
    public bool IsSystemMessage { get; internal set; }

    protected ChatMessage() { }

    public ChatMessage(Guid conversationId, Guid jobId, Guid senderId, string content, bool isSystemMessage = false)
    {
        Id = Guid.NewGuid().ToString();
        ConversationId = conversationId;
        JobId = jobId;
        SenderId = senderId;
        Content = content ?? throw new ArgumentNullException(nameof(content));
        SentAt = DateTime.UtcNow;
        IsSystemMessage = isSystemMessage;
    }

    public static ChatMessage FromPersistence(string id, Guid conversationId, Guid jobId, Guid senderId, string content, DateTime sentAt, bool isSystemMessage = false)
    {
        return new ChatMessage
        {
            Id = id,
            ConversationId = conversationId,
            JobId = jobId,
            SenderId = senderId,
            Content = content,
            SentAt = sentAt,
            IsSystemMessage = isSystemMessage
        };
    }
}
