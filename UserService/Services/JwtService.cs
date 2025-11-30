using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using UserService.Models;
using Microsoft.Extensions.Configuration;

namespace UserService.Services;

public class JwtService
{
    private readonly string _secret;
    public JwtService(IConfiguration cfg) => _secret = cfg["Jwt:Secret"] ?? throw new Exception("Jwt:Secret not set");

    public string GenerateToken(User user)
    {
        var key = Encoding.ASCII.GetBytes(_secret);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
