using backend.Api.DTOs.Jobs;
using backend.Api.Extensions;
using backend.Application.Interfaces;
using backend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> CreateJob(
        [FromBody] CreateJobRequest request)
    {
        var clientId = User.GetUserId();

        var jobId = await _jobService.CreateJob(
            clientId,
            request.Title ?? string.Empty,
            request.Description,
            request.ScheduledDate,
            request.Price,
            request.IsEmergency,
            request.ServiceCategory
        );

        return Ok(jobId);
    }

    [HttpPost("{jobId:guid}/send-requests")]
    public async Task<IActionResult> SendRequests(
        Guid jobId,
        [FromBody] SendRequestsRequest request)
    {
        await _jobService.SendRequests(jobId, request.MasterIds);
        return Ok();
    }

    [AllowAnonymous]
    [HttpGet("has-sent-request-to/{masterId:guid}")]
    public async Task<ActionResult<object>> HasSentRequestToMaster(Guid masterId)
    {
        if (User.Identity?.IsAuthenticated != true)
            return Ok(new { hasSentRequest = false });
        var (userId, role) = User.GetUserIdAndRole();
        if (role != UserRole.Client)
            return Ok(new { hasSentRequest = false });
        var hasSent = await _jobService.HasClientSentRequestToMaster(userId, masterId);
        return Ok(new { hasSentRequest = hasSent });
    }

    /// <summary>Svi poslovi za trenutnog korisnika: majstor = zahtevi na čekanju + dodeljeni, klijent = kreirani.</summary>
    [AllowAnonymous]
    [HttpGet("list")]
    public async Task<ActionResult<List<JobListItemResponse>>> GetJobs()
    {
        if (User.Identity?.IsAuthenticated != true)
            return Ok(new List<JobListItemResponse>());
        var (userId, role) = User.GetUserIdAndRole();
        var list = await _jobService.GetJobsForUser(userId, role);
        return Ok(list);
    }

    [AllowAnonymous]
    [HttpGet("marketplace")]
    public async Task<ActionResult<List<JobListItemResponse>>> GetMarketplaceJobs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (User.Identity?.IsAuthenticated != true)
            return Ok(new List<JobListItemResponse>());

        var list = await _jobService.GetMarketplaceJobs(page, pageSize);
        return Ok(list);
    }

    [HttpPost("{jobId:guid}/accept")]
    public async Task<IActionResult> AcceptJob(Guid jobId)
    {
        var masterId = User.GetUserId();
        await _jobService.AcceptJob(jobId, masterId);
        return Ok();
    }

    [HttpPost("{jobId:guid}/start")]
    public async Task<IActionResult> StartJob(Guid jobId)
    {
        var userId = User.GetUserId();
        await _jobService.StartJob(jobId, userId);
        return Ok();
    }

    [HttpPost("{jobId:guid}/complete")]
    public async Task<IActionResult> CompleteJob(Guid jobId)
    {
        var userId = User.GetUserId();
        await _jobService.CompleteJob(jobId, userId);
        return Ok();
    }

    [HttpPut("{jobId:guid}/description")]
    public async Task<IActionResult> ChangeDescription(
        Guid jobId,
        [FromBody] ChangeDescriptionRequest request)
    {
        var userId = User.GetUserId();
        await _jobService.ChangeDescription(
            jobId,
            userId,
            request.Description
        );

        return Ok();
    }

    [HttpPut("{jobId:guid}/price")]
    public async Task<IActionResult> ChangePrice(
        Guid jobId,
        [FromBody] ChangePriceRequest request)
    {
        var userId = User.GetUserId();
        await _jobService.ChangePrice(
            jobId,
            userId,
            request.Price
        );

        return Ok();
    }
}