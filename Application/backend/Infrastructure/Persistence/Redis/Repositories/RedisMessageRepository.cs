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

    public async Task Save(Message message)
    {
        var doc = ChatMessageMapper.ToEntity(message);
        var existing = await _messages.FindByIdAsync(doc.Id.ToString());
        if (existing is null)
            await _messages.InsertAsync(doc);
        else
            await _messages.UpdateAsync(doc);
    }

    public Task<List<Message>> GetByConversationId(Guid conversationId)
    {
        var docs = _messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.SentAt)
            .ToList();
        return Task.FromResult(docs.Select(ChatMessageMapper.ToDomain).ToList());
    }

    public Task<Message?> GetLastByConversationId(Guid conversationId)
    {
        var doc = _messages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.SentAt)
            .FirstOrDefault();
        return Task.FromResult(doc != null ? ChatMessageMapper.ToDomain(doc) : null);
    }

}
