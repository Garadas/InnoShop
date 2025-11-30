using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Services;

[ApiController]
[Route("api/[controller]")]
public class UserProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ProductService.Services.ProductServise _productService;
    public UserProductsController(AppDbContext db, ProductService.Services.ProductServise productService)
    {
        _db = db;
        _productService = productService;
    }

    [HttpPost("{userId}/deactivate")]
    public async Task<IActionResult> DeactivateUserProducts(Guid userId)
    {
        var products = await _db.Products
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .ToListAsync();

        foreach (var product in products)
        {
            product.IsAvailable = false;
            product.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{userId}/activate")]
    public async Task<IActionResult> ActivateUserProducts(Guid userId)
    {
        var products = await _db.Products
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .ToListAsync();

        foreach (var product in products)
        {
            product.IsAvailable = true;
            product.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }
}
