namespace backend.Domain.Entities;
using Neo4j.Driver;

public class ChatConversation
{
    public Guid Id { get; private set; }

    public Guid JobId { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid MasterId { get; private set; }

    public bool IsActive { get; private set; }

    protected ChatConversation() { }

    public ChatConversation(Guid jobId, Guid clientId, Guid masterId)
    {
        Id = Guid.NewGuid();
        JobId = jobId;
        ClientId = clientId;
        MasterId = masterId;
        IsActive = true;
    }

    // ---------------- DOMENSKE OPERACIJE ----------------

    public void Close()
    {
        IsActive = false;
    }

    public static ChatConversation Rehydrate(INode node)
    {
        var conversation = new ChatConversation();

        conversation.Id = Guid.Parse(node.Properties["id"].As<string>());
        conversation.JobId = Guid.Parse(node.Properties["jobId"].As<string>());
        conversation.ClientId = Guid.Parse(node.Properties["clientId"].As<string>());
        conversation.MasterId = Guid.Parse(node.Properties["masterId"].As<string>());
        conversation.IsActive = node.Properties["isActive"].As<bool>();

        return conversation;
    }
}
