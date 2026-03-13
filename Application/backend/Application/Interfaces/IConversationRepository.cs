using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IConversationRepository
{
    Task Save(Conversation conversation);

    Task SaveMany(IEnumerable<Conversation> conversations);

    Task<List<Conversation>> GetByJobId(Guid jobId);

    Task<Conversation?> GetById(Guid? id);

    Task<List<Conversation>> GetByUserId(Guid userId);

    /// <summary>Vraća bilo koju aktivnu konverzaciju između klijenta i majstora (sa poslom ili bez).</summary>
    Task<Conversation?> GetActiveByClientAndMaster(Guid clientId, Guid masterId);

    /// <summary>Vraća bilo koju konverzaciju između klijenta i majstora (aktivnu ili zatvorenu).</summary>
    Task<Conversation?> GetByClientAndMaster(Guid clientId, Guid masterId);

    /// <summary>Broj nepročitanih poruka za korisnika u konverzaciji.</summary>
    Task<int> GetUnreadCountAsync(Guid conversationId, Guid userId);

    /// <summary>Označi konverzaciju kao pročitanu za korisnika (otvorio je chat).</summary>
    Task MarkAsReadAsync(Guid conversationId, Guid userId);

    /// <summary>Uvećaj broj nepročitanih za primaoca kada neko pošalje poruku.</summary>
    Task IncrementUnreadAsync(Guid conversationId, Guid userId);
}
