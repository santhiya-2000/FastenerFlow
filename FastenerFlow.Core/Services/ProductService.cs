using FastenerFlow.Core.DTOs;
using FastenerFlow.Core.Models;
using FastenerFlow.Core.Repositories;

namespace FastenerFlow.Core.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product != null ? MapToDto(product) : null;
    }

    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
    {
        var products = await _productRepository.GetLowStockAsync();
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
    {
        var product = new Product
        {
            Sku = productDto.Sku,
            Name = productDto.Name,
            Category = productDto.Category,
            UnitPrice = productDto.UnitPrice,
            Quantity = productDto.Quantity,
            ReorderLevel = productDto.ReorderLevel
        };

        var createdProduct = await _productRepository.AddAsync(product);
        return MapToDto(createdProduct);
    }

    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductDto productDto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return null;

        product.Sku = productDto.Sku;
        product.Name = productDto.Name;
        product.Category = productDto.Category;
        product.UnitPrice = productDto.UnitPrice;
        product.Quantity = productDto.Quantity;
        product.ReorderLevel = productDto.ReorderLevel;

        var updatedProduct = await _productRepository.UpdateAsync(product);
        return MapToDto(updatedProduct);
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return false;

        await _productRepository.DeleteAsync(id);
        return true;
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Sku = product.Sku,
            Name = product.Name,
            Category = product.Category,
            UnitPrice = product.UnitPrice,
            Quantity = product.Quantity,
            ReorderLevel = product.ReorderLevel
        };
    }
}
