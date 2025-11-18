using Microsoft.AspNetCore.Mvc;
using MyApiProject.Models;
using MyApiProject.Repositories;

namespace MyApiProject.Controllers;

[ApiController]
[Route("api/[controller]")]
// Support V1 & V2 in same controller
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _repo;

    public UsersController(IUserRepository repo) => _repo = repo;

    // GET /api/users
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<User>>> GetAllV1([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
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

    [HttpGet]
    [MapToApiVersion("2.0")]
    public async Task<ActionResult<IEnumerable<User>>> GetAllV2([FromQuery] int page = 1, [FromQuery] int pageSize = 2)
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

    [HttpPost("search")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> SearchUsers(
    [FromBody] IEnumerable<UserSearchFilter> filters,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        var users = await _repo.SearchUsersAsync(filters, page, pageSize);

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
    [MapToApiVersion("1.0")]
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
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update(string id, [FromBody] User user)
    {
        var exists = await _repo.GetByIdAsync(id);
        if (exists is null) return NotFound();

        var ok = await _repo.UpdateAsync(id, user);
        return ok ? NoContent() : StatusCode(500, "Update failed.");
    }

    // DELETE /api/users/{id}  (requires x-api-key)
    [HttpDelete("{id:length(24)}")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(string id)
    {
        var exists = await _repo.GetByIdAsync(id);
        if (exists is null) return NotFound();

        var ok = await _repo.DeleteAsync(id);
        return ok ? NoContent() : StatusCode(500, "Delete failed.");
    }

    [HttpGet("count")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetUserCount()
    {
        var count = await _repo.GetUserCountAsync();
        return Ok(new { count });
    }

    [HttpDelete("truncate")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> TruncateUsers()
    {
        await _repo.TruncateUsersAsync();
        return Ok(new { message = "All users have been deleted." });
    }



}
