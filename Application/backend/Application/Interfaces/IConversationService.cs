using backend.Api.DTOs.Conversation;

namespace backend.Application.Interfaces;

public interface IConversationService
{
    Task<List<ConversationListItemResponse>> GetConversationsForUser(Guid userId);
    Task<ConversationListItemResponse?> GetConversationForUser(Guid userId, Guid conversationId);
    Task<List<ChatMessageResponse>> GetMessagesForUser(Guid userId, Guid conversationId);
    Task MarkAsRead(Guid userId, Guid conversationId);
    Task<ConversationCreatedResponse> EnsureOrCreateWithMaster(Guid clientId, Guid masterId);
}

