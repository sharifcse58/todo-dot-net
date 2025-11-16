using Microsoft.AspNetCore.Mvc;
using Bogus;
using MyApiProject.Models;
using MyApiProject.Repositories;
using Microsoft.Extensions.Logging;

namespace MyApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeedController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly ILogger<SeedController> _logger;

    public SeedController(IUserRepository userRepo, ILogger<SeedController> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    [HttpPost("users")]
    public async Task<IActionResult> BulkInsertUsers([FromQuery] int count = 10000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        _logger.LogInformation("Starting bulk user insertion: {Count} users", count);

        // Use Bogus to generate fake data
        var faker = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Role, f => f.Name.JobTitle());

        var fakeUsers = faker.Generate(count);

        // Avoid duplicates by ensuring emails are unique
        var uniqueUsers = fakeUsers
            .GroupBy(u => u.Email.ToLowerInvariant())
            .Select(g => g.First())
            .ToList();

        // Bulk insert into MongoDB
        //await _userRepo.BulkInsertAsync(uniqueUsers);

        // Bulk insert using loop MongoDB
        await _userRepo.BulkInsertLoopAsync(uniqueUsers);

        stopwatch.Stop();

        _logger.LogInformation("Inserted {Count} users in {Elapsed} ms",
            uniqueUsers.Count, stopwatch.ElapsedMilliseconds);

        return Ok(new
        {
            message = $"Inserted {uniqueUsers.Count} users.",
            elapsedMs = stopwatch.ElapsedMilliseconds
        });
    }
}
