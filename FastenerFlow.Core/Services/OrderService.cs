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
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            };

            foreach (var itemDto in orderDto.OrderItems)
            {
                if (itemDto.Quantity <= 0)
                {
                    throw new ArgumentException("Order quantity must be positive.");
                }

                var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                {
                    throw new ArgumentException($"Product with ID {itemDto.ProductId} not found.");
                }

                if (product.Quantity < itemDto.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product {product.Name}. Available: {product.Quantity}, Requested: {itemDto.Quantity}");
                }

                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.UnitPrice
                };

                order.OrderItems.Add(orderItem);

                product.Quantity -= itemDto.Quantity;
                _context.Products.Update(product);
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
