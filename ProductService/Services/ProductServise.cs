using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Entities;
using ProductService.Exceptions;

namespace ProductService.Services;

public class ProductServise : IProductService
{
    private readonly AppDbContext _db;

    public ProductServise(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Product> CreateProductAsync(ProductCreateDto dto, Guid userId)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            IsAvailable = dto.IsAvailable,
            CreatedAt = DateTime.UtcNow,
            UserId = userId
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateProductAsync(Guid productId, ProductUpdateDto dto, Guid userId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) throw new NotFoundException("Product not found");
        if (product.UserId != userId) throw new ForbiddenException("Cannot edit other user's product");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.IsAvailable = dto.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return product;
    }

    public async Task DeleteProductAsync(Guid productId, Guid userId)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) throw new NotFoundException("Product not found");
        if (product.UserId != userId) throw new ForbiddenException("Cannot delete other user's product");

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<Product?> GetProductByIdAsync(Guid productId)
    {
        return await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
    }

    public async Task DeactivateUserProductsAsync(Guid userId)
    {
        var products = await _db.Products
            .Where(p => p.UserId == userId && !p.IsDeleted)
            .ToListAsync();

        foreach (var product in products)
        {
            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public async Task ActivateUserProductsAsync(Guid userId)
    {
        var products = await _db.Products
            .Where(p => p.UserId == userId && p.IsDeleted)
            .ToListAsync();

        foreach (var product in products)
        {
            product.IsDeleted = false;
            product.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsAsync(ProductFilterDto? filter = null)
    {
        var query = _db.Products.AsQueryable();

        query = query.Where(p => !p.IsDeleted && p.IsAvailable);

        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Name))
                query = query.Where(p => p.Name.Contains(filter.Name));

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filter.MaxPrice.Value);

            if (filter.IsAvailable.HasValue)
                query = query.Where(p => p.IsAvailable == filter.IsAvailable.Value);

            if (filter.UserId.HasValue)
                query = query.Where(p => p.UserId == filter.UserId.Value);
        }

        return await query.ToListAsync();
    }
}
