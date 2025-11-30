using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Exceptions;
using UserService.Models;
using BCrypt.Net;
using UserService.Utils;

namespace UserService.Services;

public class AuthService
{
    private readonly UserDbContext _db;
    private readonly JwtService _jwt;

    public AuthService(UserDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<string> Register(UserCreateDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email && u.DeletedAt == null))
            throw new ConflictException("Email already registered");

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = Enum.Parse<UserRole>(dto.Role, true),
            IsActive = false,
            EmailConfirmationToken = TokenGenerator.Create()
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user.EmailConfirmationToken!;
    }

    public async Task<string> Login(LoginDto dto)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == dto.Email && u.DeletedAt == null);
        if (user == null) throw new NotFoundException("Invalid credentials");
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash)) throw new NotFoundException("Invalid credentials");
        if (!user.IsActive) throw new BadRequestException("Account inactive");
        if (user.ConfirmedAt == null) throw new BadRequestException("Email not confirmed");

        return _jwt.GenerateToken(user);
    }

    public async Task ConfirmEmail(string token)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token && u.DeletedAt == null);
        if (user == null) throw new NotFoundException("Invalid token");

        user.EmailConfirmationToken = null;
        user.ConfirmedAt = DateTime.UtcNow;
        user.IsActive = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task StartForgotPassword(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null);
        if (user == null) return; // do not reveal existence

        user.ResetPasswordToken = TokenGenerator.Create();
        user.ResetPasswordTokenExpires = DateTime.UtcNow.AddMinutes(30);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task ResetPassword(ResetPasswordDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == dto.Token && u.ResetPasswordTokenExpires != null && u.ResetPasswordTokenExpires > DateTime.UtcNow && u.DeletedAt == null);
        if (user == null) throw new BadRequestException("Invalid or expired token");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.ResetPasswordToken = null;
        user.ResetPasswordTokenExpires = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
