using FastenerFlow.Core.Data;
using FastenerFlow.Core.DTOs;
using FastenerFlow.Core.Models;
using FastenerFlow.Core.Repositories;
using FastenerFlow.Core.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FastenerFlow.Tests;

public class OrderServiceTests
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
    public async Task CreateOrder_ValidOrder_CreatesOrderAndDeductsStock()
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

        var productRepository = new ProductRepository(context);
        var orderRepository = new OrderRepository(context);
        var service = new OrderService(orderRepository, productRepository, context);

        var createDto = new CreateOrderDto
        {
            OrderItems = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 10 }
            }
        };

        var result = await service.CreateOrderAsync(createDto);

        Assert.NotNull(result);
        Assert.Single(result.OrderItems);
        Assert.Equal(10, result.OrderItems[0].Quantity);

        var updatedProduct = await productRepository.GetByIdAsync(product.Id);
        Assert.Equal(90, updatedProduct!.Quantity);
    }

    [Fact]
    public async Task CreateOrder_InsufficientStock_ThrowsException()
    {
        var context = GetInMemoryContext();
        var product = new Product
        {
            Sku = "TEST-001",
            Name = "Test Product",
            Category = "Test",
            UnitPrice = 10.99m,
            Quantity = 5,
            ReorderLevel = 20
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var productRepository = new ProductRepository(context);
        var orderRepository = new OrderRepository(context);
        var service = new OrderService(orderRepository, productRepository, context);

        var createDto = new CreateOrderDto
        {
            OrderItems = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 10 }
            }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateOrderAsync(createDto));
    }

    [Fact]
    public async Task CreateOrder_InvalidQuantity_ThrowsException()
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

        var productRepository = new ProductRepository(context);
        var orderRepository = new OrderRepository(context);
        var service = new OrderService(orderRepository, productRepository, context);

        var createDto = new CreateOrderDto
        {
            OrderItems = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 0 }
            }
        };

        await Assert.ThrowsAnyAsync<Exception>(() => service.CreateOrderAsync(createDto));
    }

    [Fact]
    public async Task CreateOrder_NonExistentProduct_ThrowsException()
    {
        var context = GetInMemoryContext();
        var productRepository = new ProductRepository(context);
        var orderRepository = new OrderRepository(context);
        var service = new OrderService(orderRepository, productRepository, context);

        var createDto = new CreateOrderDto
        {
            OrderItems = new List<CreateOrderItemDto>
            {
                new() { ProductId = 999, Quantity = 10 }
            }
        };

        await Assert.ThrowsAnyAsync<Exception>(() => service.CreateOrderAsync(createDto));
    }

    [Fact]
    public async Task CreateOrder_TransactionRollbackOnFailure_PreservesStock()
    {
        var context = GetInMemoryContext();
        var product = new Product
        {
            Sku = "TEST-001",
            Name = "Test Product",
            Category = "Test",
            UnitPrice = 10.99m,
            Quantity = 5,
            ReorderLevel = 20
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var productRepository = new ProductRepository(context);
        var orderRepository = new OrderRepository(context);
        var service = new OrderService(orderRepository, productRepository, context);

        var createDto = new CreateOrderDto
        {
            OrderItems = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 10 }
            }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateOrderAsync(createDto));

        var unchangedProduct = await productRepository.GetByIdAsync(product.Id);
        Assert.Equal(5, unchangedProduct!.Quantity);
    }

    [Fact]
    public async Task CreateOrder_RepeatedProductLines_AggregatesQuantities()
    {
        using var context = GetInMemoryContext();
        var product = new Product
        {
            Sku = "DUP-001",
            Name = "Hex Bolt",
            Category = "Bolts",
            UnitPrice = 1.25m,
            Quantity = 12,
            ReorderLevel = 2
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new OrderService(
            new OrderRepository(context), new ProductRepository(context), context);

        var order = await service.CreateOrderAsync(new CreateOrderDto
        {
            OrderItems = new List<CreateOrderItemDto>
            {
                new() { ProductId = product.Id, Quantity = 3 },
                new() { ProductId = product.Id, Quantity = 4 }
            }
        });

        Assert.Single(order.OrderItems);
        Assert.Equal(7, order.OrderItems[0].Quantity);
        Assert.Equal(5, product.Quantity);
    }

    [Fact]
    public async Task CreateOrder_RepeatedProductLinesCannotExceedStock()
    {
        using var context = GetInMemoryContext();
        var product = new Product
        {
            Sku = "DUP-002",
            Name = "Hex Nut",
            Category = "Nuts",
            UnitPrice = 0.25m,
            Quantity = 6,
            ReorderLevel = 2
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var service = new OrderService(
            new OrderRepository(context), new ProductRepository(context), context);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateOrderAsync(new CreateOrderDto
            {
                OrderItems = new List<CreateOrderItemDto>
                {
                    new() { ProductId = product.Id, Quantity = 4 },
                    new() { ProductId = product.Id, Quantity = 4 }
                }
            }));

        Assert.Equal(6, product.Quantity);
        Assert.Empty(context.Orders);
    }
}
