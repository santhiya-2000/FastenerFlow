using FastenerFlow.Core.DTOs;
using FastenerFlow.Core.Integrations;
using FastenerFlow.Core.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FastenerFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ErpController : ControllerBase
{
    private readonly IErpSyncService _erpSyncService;
    private readonly IErpSyncLogRepository _erpSyncLogRepository;

    public ErpController(
        IErpSyncService erpSyncService,
        IErpSyncLogRepository erpSyncLogRepository)
    {
        _erpSyncService = erpSyncService;
        _erpSyncLogRepository = erpSyncLogRepository;
    }

    [HttpPost("sync")]
    public async Task<ActionResult<ErpSyncResult>> SyncWithErp()
    {
        try
        {
            var result = await _erpSyncService.SyncInventoryAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new ErpSyncResult
            {
                Success = false,
                Message = $"Sync failed: {ex.Message}"
            });
        }
    }

    [HttpGet("sync-history")]
    public async Task<ActionResult<IEnumerable<ErpSyncLogDto>>> GetSyncHistory()
    {
        var logs = await _erpSyncLogRepository.GetAllAsync();
        var logDtos = logs.Select(log => new ErpSyncLogDto
        {
            Id = log.Id,
            SyncDate = log.SyncDate,
            Status = log.Status,
            ProductsUpdated = log.ProductsUpdated,
            ProductsFailed = log.ProductsFailed,
            Details = log.Details
        });

        return Ok(logDtos);
    }
}
