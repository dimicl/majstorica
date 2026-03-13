using System.Text.Json.Serialization;

namespace backend.Api.DTOs.Conversation;

public class ConversationListItemResponse
{
    public Guid Id { get; init; }
    public Guid? JobId { get; init; }
    public Guid ClientId { get; init; }
    public string? JobDescription { get; init; }
    public string OtherPartyName { get; init; } = string.Empty;
    public Guid OtherPartyId { get; init; }
    public string? LastMessageText { get; init; }
    public DateTime? LastMessageAt { get; init; }
    public bool IsActive { get; init; }

    [JsonPropertyName("unreadCount")]
    public int UnreadCount { get; init; }

    [JsonPropertyName("isOnline")]
    public bool IsOnline { get; init; }

    /// <summary>Poslednja aktivnost drugog učesnika (za prikaz u chatu).</summary>
    [JsonPropertyName("otherPartyLastSeen")]
    public DateTime? OtherPartyLastSeen { get; init; }
}
