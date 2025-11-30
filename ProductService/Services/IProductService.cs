using ProductService.DTOs;
using ProductService.Entities;

namespace ProductService.Services;

public interface IProductService
{
    Task<Product> CreateProductAsync(ProductCreateDto dto, Guid userId);
    Task<Product> UpdateProductAsync(Guid productId, ProductUpdateDto dto, Guid userId);
    Task DeleteProductAsync(Guid productId, Guid userId);
    Task<Product?> GetProductByIdAsync(Guid productId);
    Task<IEnumerable<Product>> GetProductsAsync(ProductFilterDto? filter = null);
    Task ActivateUserProductsAsync(Guid userId);
    Task DeactivateUserProductsAsync(Guid userId);
}
