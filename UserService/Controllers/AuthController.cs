using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;

namespace UserService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly EmailService _email;
    private readonly IConfiguration _cfg;

    public AuthController(AuthService auth, EmailService email, IConfiguration cfg)
    {
        _auth = auth;
        _email = email;
        _cfg = cfg;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserCreateDto dto)
    {
        var token = await _auth.Register(dto);
        var link = $"{_cfg["Email:FrontendUrl"].TrimEnd('/')}/confirm?token={token}";
        await _email.SendConfirmationEmail(dto.Email, link);
        return Ok(new { success = true });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var jwt = await _auth.Login(dto);
        return Ok(new { token = jwt });
    }

    [HttpGet("confirm")]
    public async Task<IActionResult> Confirm([FromQuery] string token)
    {
        await _auth.ConfirmEmail(token);
        return Ok(new { success = true });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] EmailDto dto)
    {
        await _auth.StartForgotPassword(dto.Email);

        var user = await _authResultHelper_GetUserByEmail(dto.Email); 
        if (!string.IsNullOrEmpty(user?.ResetPasswordToken))
        {
            var link = $"{_cfg["Email:FrontendUrl"].TrimEnd('/')}/reset-password?token={user.ResetPasswordToken}";
            await _email.SendResetPasswordEmail(dto.Email, link);
        }
        return Ok(new { success = true });
    }

    private async Task<UserService.Models.User?> _authResultHelper_GetUserByEmail(string email)
    {
        return null;
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
        await _auth.ResetPassword(dto);
        return Ok(new { success = true });
    }
}
