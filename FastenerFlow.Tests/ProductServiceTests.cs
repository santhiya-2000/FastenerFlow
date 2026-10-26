using FastenerFlow.Core.Data;
using FastenerFlow.Core.DTOs;
using FastenerFlow.Core.Models;
using FastenerFlow.Core.Repositories;
using FastenerFlow.Core.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FastenerFlow.Tests;

public class ProductServiceTests
{
    private FastenerFlowDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<FastenerFlowDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new FastenerFlowDbContext(options);
    }

    [Fact]
    public async Task CreateProduct_ValidProduct_ReturnsProduct()
    {
        var context = GetInMemoryContext();
        var repository = new ProductRepository(context);
        var service = new ProductService(repository);

        var createDto = new CreateProductDto
        {
            Sku = "TEST-001",
            Name = "Test Product",
            Category = "Test",
            UnitPrice = 10.99m,
            Quantity = 100,
            ReorderLevel = 20
        };

        var result = await service.CreateProductAsync(createDto);

        Assert.NotNull(result);
        Assert.Equal("TEST-001", result.Sku);
        Assert.Equal("Test Product", result.Name);
        Assert.Equal(100, result.Quantity);
    }

    [Fact]
    public async Task GetLowStockProducts_WithLowStock_ReturnsProducts()
    {
        var context = GetInMemoryContext();
        context.Products.AddRange(
            new Product { Sku = "A", Name = "A", Category = "Test", UnitPrice = 1, Quantity = 10, ReorderLevel = 20 },
            new Product { Sku = "B", Name = "B", Category = "Test", UnitPrice = 1, Quantity = 50, ReorderLevel = 20 }
        );
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        var service = new ProductService(repository);

        var result = await service.GetLowStockProductsAsync();

        Assert.Single(result);
        Assert.Equal("A", result.First().Sku);
    }

    [Fact]
    public async Task UpdateProduct_ValidProduct_UpdatesProduct()
    {
        var context = GetInMemoryContext();
        var product = new Product
        {
            Sku = "TEST-001",
            Name = "Test Product",
            Category = "Test",
            UnitPrice = 10.99m,
            Quantity = 100,
            ReorderLevel = 20
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        var service = new ProductService(repository);

        var updateDto = new UpdateProductDto
        {
            Sku = "TEST-001",
            Name = "Updated Product",
            Category = "Test",
            UnitPrice = 15.99m,
            Quantity = 150,
            ReorderLevel = 25
        };

        var result = await service.UpdateProductAsync(product.Id, updateDto);

        Assert.NotNull(result);
        Assert.Equal("Updated Product", result.Name);
        Assert.Equal(150, result.Quantity);
    }

    [Fact]
    public async Task DeleteProduct_ValidProduct_DeletesProduct()
    {
        var context = GetInMemoryContext();
        var product = new Product
        {
            Sku = "TEST-001",
            Name = "Test Product",
            Category = "Test",
            UnitPrice = 10.99m,
            Quantity = 100,
            ReorderLevel = 20
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        var service = new ProductService(repository);

        var result = await service.DeleteProductAsync(product.Id);

        Assert.True(result);
        var deletedProduct = await repository.GetByIdAsync(product.Id);
        Assert.Null(deletedProduct);
    }
}
