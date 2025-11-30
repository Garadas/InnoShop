using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Entities;
using ProductService.Services;
using System.Security.Claims;


namespace ProductService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Все маршруты защищены JWT
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ProductService.Services.ProductServise _productService;

        public ProductController(AppDbContext db, ProductService.Services.ProductServise productService)
        {
            _db = db;
            _productService = productService;
        }

        // ---------------- GET /api/product?filters ----------------
        [HttpGet]
        [AllowAnonymous] // Можно разрешить просмотр продуктов без авторизации
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] ProductFilterDto? filter = null)
        {
            var products = await _productService.GetProductsAsync(filter);
            return Ok(products);
        }

        // ---------------- GET /api/product/{id} ----------------
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null || product.IsDeleted)
                return NotFound();

            return Ok(product);
        }

        // ---------------- POST /api/product ----------------
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(ProductCreateDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                IsAvailable = dto.IsAvailable
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        // ---------------- PUT /api/product/{id} ----------------
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, ProductUpdateDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var product = await _db.Products.FindAsync(id);

            if (product == null || product.IsDeleted)
                return NotFound();

            if (product.UserId != userId)
                return Forbid("You can only edit your own products");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.IsAvailable = dto.IsAvailable;
            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ---------------- DELETE /api/product/{id} ----------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var product = await _db.Products.FindAsync(id);

            if (product == null || product.IsDeleted)
                return NotFound();

            if (product.UserId != userId)
                return Forbid("You can only delete your own products");

            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
