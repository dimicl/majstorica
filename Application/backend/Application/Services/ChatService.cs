using backend.Application.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Services;

public class ChatService : IChatService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationRepository _conversationRepository;

    public ChatService(
        IMessageRepository messageRepository,
        IConversationRepository conversationRepository)
    {
        _messageRepository = messageRepository;
        _conversationRepository = conversationRepository;
    }

    public async Task<ChatMessage> SendMessage(
        Guid conversationId,
        Guid jobId,
        Guid senderId,
        string content)
    {
        var conversation = await _conversationRepository.GetById(conversationId);
        if (conversation == null || !conversation.IsActive)
            throw new Exception("Chat je zatvoren.");

        var message = new ChatMessage(conversationId, jobId, senderId, content);

        await _messageRepository.Save(message);
        return message;
    }

    public Task<List<ChatMessage>> GetConversationMessages(Guid conversationId)
    {
        return _messageRepository.GetByConversationId(conversationId);
    }
}
