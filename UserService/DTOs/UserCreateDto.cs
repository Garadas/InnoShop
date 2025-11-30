using System.ComponentModel.DataAnnotations;

public class UserCreateDto
{
    [Required, MinLength(2), MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required, EmailAddress, MaxLength(191)]
    public string Email { get; set; } = null!;

    [Required, MinLength(6)]
    public string Password { get; set; } = null!;

    [Required]
    public string Role { get; set; } = "User";
}
