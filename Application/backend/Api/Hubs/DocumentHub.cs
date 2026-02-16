using System.Security.Claims;
using backend.Application.Interfaces;
using backend.Domain.Entities;
using backend.Domain.Enums;
using Microsoft.AspNetCore.SignalR;

namespace backend.Api.Hubs;

public class DocumentHub : Hub
{
    private readonly IJobService _jobService;
    private readonly IChatService _chatService;
    private readonly ISessionService _sessionService;
    private readonly IRedisLockService _lockService;

    public DocumentHub(
        IJobService jobService,
        IChatService chatService,
        ISessionService sessionService,
        IRedisLockService lockService)
    {
        _jobService = jobService;
        _chatService = chatService;
        _sessionService = sessionService;
        _lockService = lockService;
    }

    public override async Task OnConnectedAsync()
    {
        var (userId, role) = GetUserIdAndRole();
        await _sessionService.CreateOrUpdateSession(userId, role, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    private static string JobGroup(Guid jobId) => $"job:{jobId}";
    private static string ConversationGroup(Guid conversationId) => $"chat:{conversationId}";

    public async Task JoinJob(Guid jobId)
    {
        var userId = GetUserId();

        await _sessionService.MarkUserInJob(userId, jobId);

        await Groups.AddToGroupAsync(Context.ConnectionId, JobGroup(jobId));
        await Clients.Group(JobGroup(jobId))
            .SendAsync("UserJoined", userId);

        try
        {
            await _lockService.EnsureWriteAccess(jobId, userId);
            await Clients.Caller.SendAsync("WriteGranted", jobId);
        }
        catch
        {
            await Clients.Caller.SendAsync("WriteDenied", jobId);
        }
    }

    public async Task LeaveJob(Guid jobId)
    {
        var userId = GetUserId();

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, JobGroup(jobId));
        await _sessionService.MarkUserInJob(userId, Guid.Empty); //ovamo obrati paznju

        var next = await _lockService.ReleaseWriteAccess(jobId, userId);
        if (next != null)
        {
            await Clients.Group(JobGroup(jobId))
                .SendAsync("WriteGranted", jobId, next);
        }

        await Clients.Group(JobGroup(jobId))
            .SendAsync("UserLeft", userId);
    }

    public async Task JoinConversation(Guid conversationId)
    {
        var (userId, role) = GetUserIdAndRole();
        await _sessionService.CreateOrUpdateSession(userId, role, Context.ConnectionId);
        await _sessionService.MarkUserInConversation(userId, conversationId);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            ConversationGroup(conversationId)
        );
    }

    public async Task SendMessage(Guid conversationId, Guid jobId, string content)
    {
        var userId = GetUserId();

        var message = await _chatService.SendMessage(
            conversationId,
            jobId,
            userId,
            content);

        await Clients.Group(ConversationGroup(conversationId))
            .SendAsync("ReceiveMessage", message);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _sessionService.HandleDisconnect(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    private Guid GetUserId()
    {
        var (userId, _) = GetUserIdAndRole();
        return userId;
    }

    private (Guid userId, UserRole role) GetUserIdAndRole()
    {
        var userIdStr = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userIdStr))
            throw new HubException("Unauthorized");

        var roleStr = Context.User?.FindFirstValue(ClaimTypes.Role) ?? nameof(UserRole.Client);
        if (!Enum.TryParse<UserRole>(roleStr, ignoreCase: true, out var role))
            role = UserRole.Client;

        return (Guid.Parse(userIdStr), role);
    }
}
