using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using backend.Shared.Exceptions;

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

    public async Task<Message> SendMessage(
        Guid conversationId,
        Guid jobId,
        Guid senderId,
        string content)
    {
        var conversation = await _conversationRepository.GetById(conversationId);
        if (conversation == null || conversation.IsClosed)
            throw new ConflictException("Chat je zatvoren.");

        var message = new Message(conversationId, jobId, senderId, MessageType.Text, content, DateTime.UtcNow);
        await _messageRepository.Save(message);

        var recipientId = conversation.ClientUserId == senderId ? conversation.MasterUserId : conversation.ClientUserId;
        await _conversationRepository.IncrementUnreadAsync(conversationId, recipientId);

        return message;
    }

    public Task<List<Message>> GetConversationMessages(Guid conversationId)
    {
        return _messageRepository.GetByConversationId(conversationId);
    }

    public async Task<Conversation> EnsureOrCreateConversationWithMaster(Guid clientId, Guid masterId)
    {
        // Ako već postoji bilo koja aktivna konverzacija (npr. iz posla), koristi je – ne kreiraj drugi chat
        var existing = await _conversationRepository.GetActiveByClientAndMaster(clientId, masterId);
        if (existing != null)
            return existing;

        var conversation = new Conversation(
            Guid.NewGuid(),
            clientId,
            ConversationType.Direct,
            DateTime.UtcNow,
            masterId);
        await _conversationRepository.Save(conversation);
        return conversation;
    }
}
