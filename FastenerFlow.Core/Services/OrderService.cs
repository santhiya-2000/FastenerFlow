using System.Data;
using FastenerFlow.Core.Data;
using FastenerFlow.Core.DTOs;
using FastenerFlow.Core.Models;
using FastenerFlow.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FastenerFlow.Core.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly FastenerFlowDbContext _context;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        FastenerFlowDbContext context)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _context = context;
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        return order != null ? MapToDto(order) : null;
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto orderDto)
    {
        if (orderDto.OrderItems == null || orderDto.OrderItems.Count == 0)
            throw new ArgumentException("Order must contain at least one item.");

        if (orderDto.OrderItems.Any(i => i.ProductId <= 0 || i.Quantity <= 0))
            throw new ArgumentException("Order product IDs and quantities must be positive.");

        // Group repeated product IDs before checking stock to avoid split-line
        // requests bypassing quantity validation.
        var requested = orderDto.OrderItems
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(i => (long)i.Quantity) })
            .OrderBy(i => i.ProductId)
            .ToList();

        if (requested.Any(i => i.Quantity > int.MaxValue))
            throw new ArgumentException("Requested quantity exceeds the supported range.");

        // On SQL Server, serializable isolation prevents another checkout from
        // successfully reading and consuming the same stock concurrently.
        // Deadlocks may abort a checkout; the caller can retry that request.
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            };

            foreach (var item in requested)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new ArgumentException($"Product with ID {item.ProductId} not found.");

                if (product.Quantity < item.Quantity)
                    throw new InvalidOperationException(
                        $"Insufficient stock for product {product.Name}. Available: {product.Quantity}, Requested: {item.Quantity}");

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Product = product,
                    Quantity = (int)item.Quantity,
                    UnitPrice = product.UnitPrice
                });

                product.Quantity -= (int)item.Quantity;
            }

            var createdOrder = await _orderRepository.AddAsync(order);
            await transaction.CommitAsync();
            return MapToDto(createdOrder);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            Status = order.Status,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? "Unknown",
                ProductSku = oi.Product?.Sku ?? "Unknown",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice
            }).ToList()
        };
    }
}
