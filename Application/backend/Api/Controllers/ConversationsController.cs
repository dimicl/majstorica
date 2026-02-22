using backend.Api.DTOs.Conversation;
using backend.Api.Extensions;
using backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversationService;

    public ConversationsController(IConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ConversationListItemResponse>>> GetConversations()
    {
        var userId = User.GetUserId();
        var result = await _conversationService.GetConversationsForUser(userId);
        return Ok(result);
    }

    [HttpGet("{conversationId:guid}")]
    public async Task<ActionResult<ConversationListItemResponse>> GetConversation(Guid conversationId)
    {
        var userId = User.GetUserId();
        var dto = await _conversationService.GetConversationForUser(userId, conversationId);
        if (dto == null)
            return NotFound();
        return Ok(dto);
    }

    /// <summary>Označi konverzaciju kao pročitanu za trenutnog korisnika (otvorio je chat).</summary>
    [HttpPost("{conversationId:guid}/read")]
    public async Task<ActionResult> MarkAsRead(Guid conversationId)
    {
        var userId = User.GetUserId();
        await _conversationService.MarkAsRead(userId, conversationId);
        return NoContent();
    }

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<ActionResult<List<ChatMessageResponse>>> GetMessages(Guid conversationId)
    {
        var userId = User.GetUserId();
        var messages = await _conversationService.GetMessagesForUser(userId, conversationId);
        return Ok(messages);
    }

    /// <summary>
    /// Otvori ili nastavi slobodan chat sa majstorom (bez posla).
    /// Trenutni korisnik se tretira kao klijent, masterId je majstor sa kojim želi da priča.
    /// </summary>
    [HttpPost("with-master/{masterId:guid}")]
    public async Task<ActionResult<ConversationCreatedResponse>> StartConversationWithMaster(Guid masterId)
    {
        var clientId = User.GetUserId();
        var created = await _conversationService.EnsureOrCreateWithMaster(clientId, masterId);
        return CreatedAtAction(nameof(GetMessages), new { conversationId = created.Id }, created);
    }

    /// <summary>Majstor odbija zahtev za posao – zatvara konverzaciju.</summary>
    [HttpPost("{conversationId:guid}/decline")]
    public async Task<ActionResult> DeclineRequest(Guid conversationId)
    {
        var masterId = User.GetUserId();
        await _conversationService.DeclineRequest(masterId, conversationId);
        return NoContent();
    }
}
