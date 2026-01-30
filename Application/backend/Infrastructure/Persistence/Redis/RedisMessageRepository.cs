using backend.Application.Interfaces;
using backend.Domain.Entities;
using Redis.OM;
using Redis.OM.Searching;

namespace backend.Infrastructure.Persistence.Redis;

public class RedisMessageRepository : IMessageRepository
{
    private readonly RedisCollection<ChatMessage> _messages;

    public RedisMessageRepository(RedisConnectionProvider provider)
    {
        _messages = (RedisCollection<ChatMessage>)provider.RedisCollection<ChatMessage>();
    }

    public async Task Save(ChatMessage message)
    {
        await _messages.InsertAsync(message);
    }

    public Task<List<ChatMessage>> GetByConversationId(Guid conversationId)
    {
        return Task.FromResult(
            _messages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .ToList()
        );
    }
}
