namespace FastenerFlow.Core.DTOs;

public class ErpSyncLogDto
{
    public int Id { get; set; }
    public DateTime SyncDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ProductsUpdated { get; set; }
    public int ProductsFailed { get; set; }
    public string? Details { get; set; }
}
