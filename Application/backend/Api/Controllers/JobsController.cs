/* using backend.Api.DTOs.Jobs;
using backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        var clientId = GetUserId();

        var jobId = await _jobService.CreateJob(
            clientId,
            request.Description,
            request.Price,
            request.IsEmergency
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


    [HttpPost("{jobId:guid}/accept")]
    public async Task<IActionResult> AcceptJob(Guid jobId)
    {
        var masterId = GetUserId();

        await _jobService.AcceptJob(jobId, masterId);
        return Ok();
    }


    [HttpPost("{jobId:guid}/start")]
    public async Task<IActionResult> StartJob(Guid jobId)
    {
        await _jobService.StartJob(jobId);
        return Ok();
    }


    [HttpPost("{jobId:guid}/complete")]
    public async Task<IActionResult> CompleteJob(Guid jobId)
    {
        await _jobService.CompleteJob(jobId);
        return Ok();
    }


    [HttpPut("{jobId:guid}/description")]
    public async Task<IActionResult> ChangeDescription(
        Guid jobId,
        [FromBody] ChangeDescriptionRequest request)
    {
        var userId = GetUserId();

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
        var userId = GetUserId();

        await _jobService.ChangePrice(
            jobId,
            userId,
            request.Price
        );

        return Ok();
    }


    private Guid GetUserId()
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub");

        return Guid.Parse(userId!);
    }
}
 */