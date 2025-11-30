using Microsoft.AspNetCore.Mvc;
using UserService.Data;
using UserService.Models;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _db;
    private readonly ProductClient _productClient;

    public UsersController(UserDbContext db, ProductClient productClient)
    {
        _db = db;
        _productClient = productClient;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(UserCreateDto dto)
    {
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _productClient.DeactivateUserProductsAsync(user.Id);
        return NoContent();
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _productClient.ActivateUserProductsAsync(user.Id);
        return NoContent();
    }
}
