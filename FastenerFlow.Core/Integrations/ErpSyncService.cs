using FastenerFlow.Core.Data;
using FastenerFlow.Core.Models;
using FastenerFlow.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FastenerFlow.Core.Integrations;

public class ErpSyncService : IErpSyncService
{
    private readonly HttpClient _httpClient;
    private readonly IProductRepository _productRepository;
    private readonly FastenerFlowDbContext _context;
    private readonly ILogger<ErpSyncService> _logger;
    private readonly string _erpBaseUrl;

    public ErpSyncService(
        HttpClient httpClient,
        IProductRepository productRepository,
        FastenerFlowDbContext context,
        ILogger<ErpSyncService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _productRepository = productRepository;
        _context = context;
        _logger = logger;
        _erpBaseUrl = configuration["ErpSettings:BaseUrl"] ?? "https://localhost:5003";
    }

    public async Task<ErpSyncResult> SyncInventoryAsync()
    {
        var result = new ErpSyncResult();
        var syncLog = new ErpSyncLog
        {
            SyncDate = DateTime.UtcNow,
            Status = "In Progress"
        };

        try
        {
            _logger.LogInformation("Starting ERP inventory sync at {SyncTime}", DateTime.UtcNow);

            _httpClient.BaseAddress = new Uri(_erpBaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.GetAsync("/api/inventory");
            response.EnsureSuccessStatusCode();

            var erpItems = await response.Content.ReadFromJsonAsync<List<ErpInventoryItemDto>>();
            if (erpItems == null)
            {
                throw new InvalidOperationException("No inventory data received from ERP");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var erpItem in erpItems)
                {
                    var product = await _productRepository.GetBySkuAsync(erpItem.Sku);

                    if (product != null)
                    {
                        var oldQuantity = product.Quantity;
                        var quantityDiff = erpItem.Quantity - oldQuantity;

                        if (quantityDiff != 0)
                        {
                            product.Quantity = erpItem.Quantity;
                            await _productRepository.UpdateAsync(product);
                            result.ProductsUpdated++;
                            result.Discrepancies.Add(
                                $"SKU {erpItem.Sku}: Quantity adjusted by {quantityDiff} (was {oldQuantity}, now {erpItem.Quantity})");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Product with SKU {Sku} not found in local inventory", erpItem.Sku);
                        result.ProductsFailed++;
                        result.Discrepancies.Add($"SKU {erpItem.Sku}: Not found in local inventory");
                    }
                }

                syncLog.Status = "Success";
                syncLog.ProductsUpdated = result.ProductsUpdated;
                syncLog.ProductsFailed = result.ProductsFailed;
                syncLog.Details = string.Join("; ", result.Discrepancies.Take(10));

                _context.ErpSyncLogs.Add(syncLog);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                result.Success = true;
                result.Message = $"Sync completed: {result.ProductsUpdated} products updated, {result.ProductsFailed} failed";

                _logger.LogInformation("ERP sync completed successfully: {Updated} updated, {Failed} failed",
                    result.ProductsUpdated, result.ProductsFailed);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception ex)
        {
            syncLog.Status = "Failed";
            syncLog.ProductsFailed = result.ProductsFailed;
            syncLog.Details = ex.Message;
            _context.ErpSyncLogs.Add(syncLog);
            await _context.SaveChangesAsync();

            result.Success = false;
            result.Message = $"Sync failed: {ex.Message}";
            _logger.LogError(ex, "ERP sync failed");
        }

        return result;
    }
}
