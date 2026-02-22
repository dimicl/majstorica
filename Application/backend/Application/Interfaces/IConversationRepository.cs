using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IConversationRepository
{
    Task Save(ChatConversation conversation);

    Task SaveMany(IEnumerable<ChatConversation> conversations);

    Task<List<ChatConversation>> GetByJobId(Guid jobId);

    Task<ChatConversation?> GetById(Guid id);

    Task<List<ChatConversation>> GetByUserId(Guid userId);

    Task<ChatConversation?> GetByClientAndMasterAndJob(Guid clientId, Guid masterId, Guid jobId);

    /// <summary>Vraća bilo koju aktivnu konverzaciju između klijenta i majstora (sa poslom ili bez).</summary>
    Task<ChatConversation?> GetActiveByClientAndMaster(Guid clientId, Guid masterId);

    /// <summary>Vraća bilo koju konverzaciju između klijenta i majstora (aktivnu ili zatvorenu).</summary>
    Task<ChatConversation?> GetByClientAndMaster(Guid clientId, Guid masterId);

    /// <summary>Da li postoji aktivna konverzacija (aktivan posao/zahtev) između klijenta i majstora.</summary>
    Task<bool> ExistsByClientAndMaster(Guid clientId, Guid masterId);

    /// <summary>Broj nepročitanih poruka za korisnika u konverzaciji.</summary>
    Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId);

    /// <summary>Označi konverzaciju kao pročitanu za korisnika (otvorio je chat).</summary>
    Task MarkAsReadAsync(Guid conversationId, Guid userId);

    /// <summary>Uvećaj broj nepročitanih za primaoca kada neko pošalje poruku.</summary>
    Task IncrementUnreadAsync(Guid conversationId, Guid userId);
}
