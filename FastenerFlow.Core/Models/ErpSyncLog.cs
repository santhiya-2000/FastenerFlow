namespace FastenerFlow.Core.Models;

public class ErpSyncLog
{
    public int Id { get; set; }
    public DateTime SyncDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = string.Empty;
    public int ProductsUpdated { get; set; }
    public int ProductsFailed { get; set; }
    public string? Details { get; set; }
}
