namespace FastenerFlow.Core.Integrations;

public interface IErpSyncService
{
    Task<ErpSyncResult> SyncInventoryAsync();
}

public class ErpSyncResult
{
    public bool Success { get; set; }
    public int ProductsUpdated { get; set; }
    public int ProductsFailed { get; set; }
    public List<string> Discrepancies { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
