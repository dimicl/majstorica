using backend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Api.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly IJobRepository _jobRepository;

    public TestController(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    [HttpPost("job")]
    public async Task<IActionResult> CreateJob([FromQuery] string description)
    {
        var job = new backend.Domain.Entities.Job(Guid.NewGuid(), description);

        await _jobRepository.Save(job);

        return Ok(new
        {
            message = "Job saved to Neo4j",
            jobId = job.Id
        });
    }

    // 2️⃣ Čitanje Job-a iz Neo4j
    [HttpGet("job/{id}")]
    public async Task<IActionResult> GetJob(Guid id)
    {
        var job = await _jobRepository.GetById(id);

        if (job == null)
            return NotFound("Job not found");

        return Ok(new
        {
            job.Id,
            job.Description,
            Status = job.Status.ToString()
        });
    }
}
