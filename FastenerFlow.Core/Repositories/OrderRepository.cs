using FastenerFlow.Core.Data;
using FastenerFlow.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FastenerFlow.Core.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly FastenerFlowDbContext _context;

    public OrderRepository(FastenerFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }
}
