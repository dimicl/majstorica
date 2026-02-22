namespace backend.Application.Messages;

public record NewJobRequestMessage(
    Guid JobId,
    Guid ConversationId,
    string JobTitle,
    string Description,
    DateTime Date,
    string ClientName,
    Guid ClientId,
    decimal? Price,
    bool IsEmergency);
