using FastenerFlow.Core.Models;

namespace FastenerFlow.Core.Repositories;

public interface IErpSyncLogRepository
{
    Task<IEnumerable<ErpSyncLog>> GetAllAsync();
    Task<ErpSyncLog?> GetLatestAsync();
}
