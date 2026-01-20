using backend.Application.Services;
using Microsoft.AspNetCore.Mvc;
using backend.Application.Interfaces;


namespace backend.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    //Kreiranje posla
    [HttpPost]
    public async Task<IActionResult> CreateJob([FromQuery] Guid clientId, [FromQuery] string description)
    {
        var jobId = await _jobService.CreateJob(clientId, description);

        return Ok(new
        {
            jobId
        });
    }

    //Dodela majstora (Created -> InProgress)
    [HttpPost("{id}/assign")]
    public async Task<IActionResult> AssignMaster(Guid id, [FromQuery] Guid masterId)
    {
        await _jobService.AssignMaster(id, masterId);

        return Ok("Dodeljen majstor");
    }

    //Izmena opisa uz Redis lock
    [HttpPost("{id}/description")]
    public async Task<IActionResult> ChangeDescription(
        Guid id,
        [FromQuery] string description,
        [FromQuery] Guid userId)
    {
        await _jobService.ChangeDescription(id, description, userId);

        return Ok("Opis promenjen");
    }

    //Start posla
    [HttpPost("{id}/start")]
    public async Task<IActionResult> StartJob(Guid id)
    {
        await _jobService.StartJob(id);

        return Ok("Posao zapocet");
    }

    //Završetak posla
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteJob(Guid id)
    {
        await _jobService.CompleteJob(id);

        return Ok("Posao zavrsen");
    }
}
