using FastenerFlow.Core.Data;
using FastenerFlow.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace FastenerFlow.Core.Repositories;

public class ErpSyncLogRepository : IErpSyncLogRepository
{
    private readonly FastenerFlowDbContext _context;

    public ErpSyncLogRepository(FastenerFlowDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ErpSyncLog>> GetAllAsync()
    {
        return await _context.ErpSyncLogs
            .OrderByDescending(e => e.SyncDate)
            .ToListAsync();
    }

    public async Task<ErpSyncLog?> GetLatestAsync()
    {
        return await _context.ErpSyncLogs
            .OrderByDescending(e => e.SyncDate)
            .FirstOrDefaultAsync();
    }
}
