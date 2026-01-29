using backend.Domain.Entities;

namespace backend.Application.Interfaces;

public interface IConversationRepository
{
    Task Save(ChatConversation conversation);

    Task SaveMany(IEnumerable<ChatConversation> conversations);

    Task<List<ChatConversation>> GetByJobId(Guid jobId);

    Task<ChatConversation?> GetById(Guid id);
}
