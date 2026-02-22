using backend.Api.DTOs.Conversation;
using backend.Application.Helpers;
using backend.Application.Interfaces;
using backend.Domain.Entities;

namespace backend.Application.Services;

public class ConversationService : IConversationService
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJobRepository _jobRepository;
    private readonly ISessionService _sessionService;
    private readonly IChatService _chatService;

    public ConversationService(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IUserRepository userRepository,
        IJobRepository jobRepository,
        ISessionService sessionService,
        IChatService chatService)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _userRepository = userRepository;
        _jobRepository = jobRepository;
        _sessionService = sessionService;
        _chatService = chatService;
    }

    public async Task<List<ConversationListItemResponse>> GetConversationsForUser(Guid userId)
    {
        var conversations = await _conversationRepository.GetByUserId(userId);
        var result = new List<ConversationListItemResponse>();

        foreach (var conv in conversations)
        {
            var otherPartyId = conv.ClientId == userId ? conv.MasterId : conv.ClientId;
            var otherUser = await _userRepository.GetById(otherPartyId);
            var job = conv.JobId != Guid.Empty
                ? await _jobRepository.GetById(conv.JobId)
                : null;

            ChatMessage? lastMessage = null;
            try
            {
                lastMessage = await _messageRepository.GetLastByConversationId(conv.Id);
            }
            catch
            {
                // Redis "no such index" ili drugi problem – nastavljamo bez lastMessage
            }

            var unreadCount = await _conversationRepository.GetUnreadCountAsync(conv.Id, userId);
            var isOnline = await _sessionService.IsUserOnlineAsync(otherPartyId);

            result.Add(new ConversationListItemResponse
            {
                Id = conv.Id,
                JobId = conv.JobId,
                ClientId = conv.ClientId,
                JobDescription = job?.Description,
                OtherPartyName = UserDisplayNameHelper.GetDisplayName(otherUser, "Korisnik"),
                OtherPartyId = otherPartyId,
                LastMessageText = lastMessage?.Content,
                LastMessageAt = lastMessage?.SentAt,
                IsActive = conv.IsActive,
                UnreadCount = unreadCount,
                IsOnline = isOnline
            });
        }

        return result;
    }

    public async Task<ConversationListItemResponse?> GetConversationForUser(Guid userId, Guid conversationId)
    {
        var conversation = await _conversationRepository.GetById(conversationId);
        if (conversation == null)
            return null;
        if (conversation.ClientId != userId && conversation.MasterId != userId)
            throw new UnauthorizedAccessException();

        var otherPartyId = conversation.ClientId == userId ? conversation.MasterId : conversation.ClientId;
        var otherUser = await _userRepository.GetById(otherPartyId);
        var job = conversation.JobId != Guid.Empty
            ? await _jobRepository.GetById(conversation.JobId)
            : null;

        ChatMessage? lastMessage = null;
        try
        {
            lastMessage = await _messageRepository.GetLastByConversationId(conversationId);
        }
        catch
        {
            // ignore
        }

        var unreadCount = await _conversationRepository.GetUnreadCountAsync(conversationId, userId);
        var isOnline = await _sessionService.IsUserOnlineAsync(otherPartyId);

        return new ConversationListItemResponse
        {
            Id = conversation.Id,
            JobId = conversation.JobId,
            JobDescription = job?.Description,
            OtherPartyName = UserDisplayNameHelper.GetDisplayName(otherUser, "Korisnik"),
            OtherPartyId = otherPartyId,
            LastMessageText = lastMessage?.Content,
            LastMessageAt = lastMessage?.SentAt,
            IsActive = conversation.IsActive,
            UnreadCount = unreadCount,
            IsOnline = isOnline
        };
    }

    public async Task<List<ChatMessageResponse>> GetMessagesForUser(Guid userId, Guid conversationId)
    {
        var conversation = await _conversationRepository.GetById(conversationId);
        if (conversation == null)
            throw new KeyNotFoundException("Conversation not found.");
        if (conversation.ClientId != userId && conversation.MasterId != userId)
            throw new UnauthorizedAccessException();

        var messages = await _messageRepository.GetByConversationId(conversationId);
        return messages.Select(m => new ChatMessageResponse
        {
            Id = m.Id,
            ConversationId = m.ConversationId,
            JobId = m.JobId,
            SenderId = m.SenderId,
            Content = m.Content,
            SentAt = m.SentAt,
            IsSystemMessage = m.IsSystemMessage
        }).ToList();
    }

    public async Task MarkAsRead(Guid userId, Guid conversationId)
    {
        var conversation = await _conversationRepository.GetById(conversationId);
        if (conversation == null)
            throw new KeyNotFoundException("Conversation not found.");
        if (conversation.ClientId != userId && conversation.MasterId != userId)
            throw new UnauthorizedAccessException();

        await _conversationRepository.MarkAsReadAsync(conversationId, userId);
    }

    public async Task<ConversationCreatedResponse> EnsureOrCreateWithMaster(Guid clientId, Guid masterId)
    {
        var conversation = await _chatService.EnsureOrCreateConversationWithMaster(clientId, masterId);
        return new ConversationCreatedResponse { Id = conversation.Id };
    }

    public async Task DeclineRequest(Guid masterId, Guid conversationId)
    {
        var conversation = await _conversationRepository.GetById(conversationId);
        if (conversation == null)
            throw new KeyNotFoundException("Konverzacija nije pronađena.");
        if (conversation.MasterId != masterId)
            throw new UnauthorizedAccessException("Samo majstor može da odbije zahtev.");

        var master = await _userRepository.GetById(masterId);
        var masterName = UserDisplayNameHelper.GetDisplayName(master, "Majstor");
        var systemMessage = new ChatMessage(
            conversationId,
            conversation.JobId,
            masterId,
            $"{masterName} je odbio zahtev za posao.",
            isSystemMessage: true);
        await _messageRepository.Save(systemMessage);

        conversation.Close();
        await _conversationRepository.Save(conversation);
    }

    public async Task<Guid?> GetRecipientId(Guid conversationId, Guid currentUserId)
    {
        var conversation = await _conversationRepository.GetById(conversationId);
        if (conversation == null) return null;
        return conversation.ClientId == currentUserId ? conversation.MasterId : conversation.ClientId;
    }
}

