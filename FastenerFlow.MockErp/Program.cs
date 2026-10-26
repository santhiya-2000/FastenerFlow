using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

var mockErpInventory = new List<ErpInventoryItem>
{
    new("ERP001", "HB-M8", "Hex Bolt M8", 150, 0.85m),
    new("ERP002", "HB-M10", "Hex Bolt M10", 200, 1.25m),
    new("ERP003", "SSN-M8", "Stainless Steel Nut M8", 300, 0.45m),
    new("ERP004", "FW-M10", "Flat Washer M10", 500, 0.15m),
    new("ERP005", "WS-40mm", "Wood Screw 40mm", 800, 0.35m)
};

app.MapGet("/api/inventory", () => Results.Ok(mockErpInventory))
    .WithName("GetInventory")
    .WithOpenApi()
    .WithTags("Inventory");

app.MapGet("/api/inventory/{itemId}", (string itemId) =>
{
    var item = mockErpInventory.FirstOrDefault(i => i.ItemId == itemId);
    return item is not null ? Results.Ok(item) : Results.NotFound();
})
    .WithName("GetInventoryItem")
    .WithOpenApi()
    .WithTags("Inventory");

app.MapPost("/api/inventory/sync-trigger", () =>
{
    return Results.Ok(new { message = "ERP sync triggered successfully", timestamp = DateTime.UtcNow });
})
    .WithName("TriggerSync")
    .WithOpenApi()
    .WithTags("Sync");

app.Run();

record ErpInventoryItem(string ItemId, string Sku, string Name, int Quantity, decimal UnitPrice);
