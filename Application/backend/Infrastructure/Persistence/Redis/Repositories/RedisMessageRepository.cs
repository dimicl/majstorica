using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Infrastructure.Persistence.Redis.Entities;
using backend.Infrastructure.Persistence.Redis.Mappers;
using Redis.OM;
using Redis.OM.Searching;

namespace backend.Infrastructure.Persistence.Redis;

public class RedisMessageRepository : IMessageRepository
{
    private readonly RedisCollection<ChatMessageDocument> _messages;

    public RedisMessageRepository(RedisConnectionProvider provider)
    {
        _messages = (RedisCollection<ChatMessageDocument>)provider.RedisCollection<ChatMessageDocument>();
    }

    public async Task Save(ChatMessage message)
    {
        var doc = ChatMessageMapper.ToEntity(message);
        await _messages.InsertAsync(doc);
    }

    public Task<List<ChatMessage>> GetByConversationId(Guid conversationId)
    {
        var docs = _messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.SentAt)
            .ToList();
        return Task.FromResult(docs.Select(ChatMessageMapper.ToDomain).ToList());
    }
}
