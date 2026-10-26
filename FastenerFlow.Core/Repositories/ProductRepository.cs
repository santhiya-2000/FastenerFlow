using FastenerFlow.Core.Data;
using FastenerFlow.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FastenerFlow.Core.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly FastenerFlowDbContext _context;

    public ProductRepository(FastenerFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _context.Products.FirstOrDefaultAsync(p => p.Sku == sku);
    }

    public async Task<IEnumerable<Product>> GetLowStockAsync()
    {
        return await _context.Products
            .Where(p => p.Quantity <= p.ReorderLevel)
            .ToListAsync();
    }

    public async Task<Product> AddAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(int id)
    {
        var product = await GetByIdAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
