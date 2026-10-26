using FastenerFlow.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FastenerFlow.Core.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(FastenerFlowDbContext context)
    {
        if (await context.Products.AnyAsync())
        {
            return;
        }

        var products = new List<Product>
        {
            new()
            {
                Sku = "HB-M8",
                Name = "Hex Bolt M8",
                Category = "Bolts",
                UnitPrice = 0.85m,
                Quantity = 100,
                ReorderLevel = 20
            },
            new()
            {
                Sku = "HB-M10",
                Name = "Hex Bolt M10",
                Category = "Bolts",
                UnitPrice = 1.25m,
                Quantity = 150,
                ReorderLevel = 25
            },
            new()
            {
                Sku = "SSN-M8",
                Name = "Stainless Steel Nut M8",
                Category = "Nuts",
                UnitPrice = 0.45m,
                Quantity = 200,
                ReorderLevel = 30
            },
            new()
            {
                Sku = "FW-M10",
                Name = "Flat Washer M10",
                Category = "Washers",
                UnitPrice = 0.15m,
                Quantity = 15,
                ReorderLevel = 50
            },
            new()
            {
                Sku = "WS-40mm",
                Name = "Wood Screw 40mm",
                Category = "Screws",
                UnitPrice = 0.35m,
                Quantity = 300,
                ReorderLevel = 40
            }
        };

        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();
    }
}
