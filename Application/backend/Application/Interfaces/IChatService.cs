using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IChatService
{
    Task<Message> SendMessage(
        Guid conversationId,
        Guid? jobId,
        Guid senderId,
        string content);

    Task<List<Message>> GetConversationMessages(Guid conversationId);

    /// <summary>
    /// Vrati ili kreira konverzaciju klijent–majstor bez posla (slobodan chat).
    /// Koristi JobId = Guid.Empty.
    /// </summary>
    Task<Conversation> EnsureOrCreateConversationWithMaster(Guid clientId, Guid masterId);
}
