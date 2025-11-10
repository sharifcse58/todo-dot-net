using Microsoft.AspNetCore.Mvc;
using MyApiProject.Models;
using MyApiProject.Repositories;

namespace MyApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;

    public UsersController(IUserRepository repo) => _repo = repo;

    // GET /api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var users = await _repo.GetAllAsync(page, pageSize);
        return Ok(new
        {
            page,
            pageSize,
            count = users.Count(),
            data = users
        });
    }


    // GET /api/users/{id}
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<User>> GetById(string id)
    {
        var user = await _repo.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    // POST /api/users  (requires x-api-key)
    [HttpPost]
    public async Task<ActionResult<User>> Create([FromBody] User user)
    {
        // [ApiController] automatically validates ModelState and returns 400 if invalid
        var created = await _repo.CreateAsync(user);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT /api/users/{id}  (requires x-api-key)
    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, [FromBody] User user)
    {
        var exists = await _repo.GetByIdAsync(id);
        if (exists is null) return NotFound();

        var ok = await _repo.UpdateAsync(id, user);
        return ok ? NoContent() : StatusCode(500, "Update failed.");
    }

    // DELETE /api/users/{id}  (requires x-api-key)
    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var exists = await _repo.GetByIdAsync(id);
        if (exists is null) return NotFound();

        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : StatusCode(500, "Delete failed.");
    }

    [HttpGet("count")]
    public async Task<IActionResult> GetUserCount()
    {
        var count = await _repo.GetUserCountAsync();
        return Ok(new { count });
    }

    [HttpDelete("truncate")]
    public async Task<IActionResult> TruncateUsers()
    {
        await _repo.TruncateUsersAsync();
        return Ok(new { message = "All users have been deleted." });
    }



}
