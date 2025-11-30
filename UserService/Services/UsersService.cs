using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Exceptions;

namespace UserService.Services;

public class UsersService
{
    private readonly UserDbContext _db;

    public UsersService(UserDbContext db) => _db = db;

    public async Task<IEnumerable<object>> GetAll() =>
        await _db.Users.Where(u => u.DeletedAt == null).Select(u => new { u.Id, u.Name, u.Email, u.Role, u.IsActive, u.CreatedAt }).ToListAsync();

    public async Task<object> GetById(Guid id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null || u.DeletedAt != null) throw new NotFoundException("User not found");
        return new { u.Id, u.Name, u.Email, u.Role, u.IsActive, u.CreatedAt, u.ConfirmedAt };
    }

    public async Task Update(Guid id, UserUpdateDto dto)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null || u.DeletedAt != null) throw new NotFoundException("User not found");

        if (!string.IsNullOrWhiteSpace(dto.Name)) u.Name = dto.Name;
        if (!string.IsNullOrWhiteSpace(dto.Role)) u.Role = Enum.Parse<Models.UserRole>(dto.Role, true);
        if (dto.IsActive.HasValue) u.IsActive = dto.IsActive.Value;
        u.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        var u = await _db.Users.FindAsync(id);
        if (u == null || u.DeletedAt != null) throw new NotFoundException("User not found");

        u.DeletedAt = DateTime.UtcNow;
        u.IsActive = false;
        await _db.SaveChangesAsync();
    }
}
