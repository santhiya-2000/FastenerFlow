using FastenerFlow.Core.DTOs;

namespace FastenerFlow.Core.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
    Task<ProductDto> CreateProductAsync(CreateProductDto productDto);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto productDto);
    Task<bool> DeleteProductAsync(int id);
}
