namespace FastenerFlow.Core.Integrations;

public class ErpInventoryItemDto
{
    public string ItemId { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
