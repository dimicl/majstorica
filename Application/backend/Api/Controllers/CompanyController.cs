using backend.Api.DTOs.Auth;
using backend.Api.DTOs.Company;
using backend.Api.Extensions;
using backend.Application.Interfaces;
using backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IAuthService _authService;

    public CompanyController(ICompanyService companyService, IAuthService authService)
    {
        _companyService = companyService;
        _authService = authService;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<CompanyResponse>> GetMine()
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.CompanyOwner)
            return Forbid();

        var response = await _companyService.GetMineForOwner(userId);
        if (response == null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CompanyResponse>> Create([FromBody] CreateCompanyRequest request)
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.CompanyOwner)
            return Forbid();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _companyService.CreateForOwner(
            userId,
            request.Name,
            request.PhoneNumber,
            request.Email,
            request.Street,
            request.City);

        return Ok(response);
    }

    [HttpGet("mine/masters/search")]
    public async Task<ActionResult<List<MasterSearchForInviteResponse>>> SearchMastersForInvite(
        [FromQuery] string? q,
        [FromQuery] int limit = 15)
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.CompanyOwner)
            return Forbid();

        var list = await _companyService.SearchMastersForInvite(userId, q, limit);
        return Ok(list);
    }

    [HttpPost("mine/invitations")]
    public async Task<IActionResult> InviteMaster([FromBody] InviteMasterRequest request)
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.CompanyOwner)
            return Forbid();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _companyService.InviteMaster(userId, request.MasterUserId);
        return NoContent();
    }

    /// <summary>Majstori kojima je ova firma poslala poziv koji je još uvek na čekanju.</summary>
    [HttpGet("mine/invitations/pending-recipients")]
    public async Task<ActionResult<List<string>>> GetPendingOutboundInviteRecipientIds()
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.CompanyOwner)
            return Forbid();

        var ids = await _companyService.GetPendingOutboundInviteMasterIdsForOwner(userId);
        return Ok(ids.ConvertAll(g => g.ToString()));
    }

    [HttpGet("mine/workers")]
    public async Task<ActionResult<List<CompanyWorkerMemberResponse>>> GetMyCompanyWorkers()
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.CompanyOwner)
            return Forbid();

        var list = await _companyService.GetWorkersForCompanyOwner(userId);
        return Ok(list);
    }

    [HttpGet("invitations/mine-pending")]
    public async Task<ActionResult<List<CompanyInvitationPendingResponse>>> GetMyPendingInvitations()
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.Master)
            return Forbid();

        var list = await _companyService.GetPendingInvitationsForMaster(userId);
        return Ok(list);
    }

    [HttpPost("invitations/{invitationId:guid}/accept")]
    public async Task<ActionResult<AuthResponse>> AcceptInvitation(Guid invitationId)
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.Master)
            return Forbid();

        await _companyService.AcceptInvitation(userId, invitationId);
        var auth = await _authService.RefreshTokenAsync(userId);
        return Ok(auth);
    }

    [HttpPost("invitations/{invitationId:guid}/decline")]
    public async Task<IActionResult> DeclineInvitation(Guid invitationId)
    {
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.Master)
            return Forbid();

        await _companyService.DeclineInvitation(userId, invitationId);
        return NoContent();
    }
}
