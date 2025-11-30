using System.ComponentModel.DataAnnotations;

namespace ProductService.DTOs;

public class ProductCreateDto
{
    [Required, MinLength(3)]
    public string Name { get; set; } = null!;

    [Required, MinLength(5)]
    public string Description { get; set; } = null!;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;
}
