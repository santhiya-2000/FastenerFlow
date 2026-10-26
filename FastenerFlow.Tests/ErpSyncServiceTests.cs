using FastenerFlow.Core.Data;
using FastenerFlow.Core.Integrations;
using FastenerFlow.Core.Models;
using FastenerFlow.Core.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using Xunit;

namespace FastenerFlow.Tests;

public class ErpSyncServiceTests
{
    private FastenerFlowDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<FastenerFlowDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new FastenerFlowDbContext(options);
    }

    [Fact]
    public async Task SyncInventoryAsync_MatchingSkus_UpdatesQuantities()
    {
        var context = GetInMemoryContext();
        var product = new Product
        {
            Sku = "HB-M8",
            Name = "Hex Bolt M8",
            Category = "Bolts",
            UnitPrice = 0.85m,
            Quantity = 100,
            ReorderLevel = 20
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"[
                    {""ItemId"":""ERP001"",""Sku"":""HB-M8"",""Name"":""Hex Bolt M8"",""Quantity"":150,""UnitPrice"":0.85}
                ]")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var productRepository = new ProductRepository(context);
        var mockLogger = new Mock<ILogger<ErpSyncService>>();
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["ErpSettings:BaseUrl"]).Returns("https://localhost:5003");

        var service = new ErpSyncService(
            httpClient,
            productRepository,
            context,
            mockLogger.Object,
            mockConfiguration.Object);

        var result = await service.SyncInventoryAsync();

        Assert.True(result.Success);
        Assert.Equal(1, result.ProductsUpdated);
        Assert.Equal(0, result.ProductsFailed);

        var updatedProduct = await productRepository.GetByIdAsync(product.Id);
        Assert.Equal(150, updatedProduct!.Quantity);

        await context.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task SyncInventoryAsync_UnmatchedSku_RecordsFailure()
    {
        var context = GetInMemoryContext();
        var product = new Product
        {
            Sku = "HB-M8",
            Name = "Hex Bolt M8",
            Category = "Bolts",
            UnitPrice = 0.85m,
            Quantity = 100,
            ReorderLevel = 20
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"[
                    {""ItemId"":""ERP001"",""Sku"":""HB-M10"",""Name"":""Hex Bolt M10"",""Quantity"":150,""UnitPrice"":1.25}
                ]")
            });

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var productRepository = new ProductRepository(context);
        var mockLogger = new Mock<ILogger<ErpSyncService>>();
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["ErpSettings:BaseUrl"]).Returns("https://localhost:5003");

        var service = new ErpSyncService(
            httpClient,
            productRepository,
            context,
            mockLogger.Object,
            mockConfiguration.Object);

        var result = await service.SyncInventoryAsync();

        Assert.True(result.Success);
        Assert.Equal(0, result.ProductsUpdated);
        Assert.Equal(1, result.ProductsFailed);

        await context.Database.EnsureDeletedAsync();
    }

    [Fact]
    public async Task SyncInventoryAsync_ErpFailure_RecordsSyncLogAsFailed()
    {
        var context = GetInMemoryContext();

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("ERP connection failed"));

        var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        var productRepository = new ProductRepository(context);
        var mockLogger = new Mock<ILogger<ErpSyncService>>();
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["ErpSettings:BaseUrl"]).Returns("https://localhost:5003");

        var service = new ErpSyncService(
            httpClient,
            productRepository,
            context,
            mockLogger.Object,
            mockConfiguration.Object);

        var result = await service.SyncInventoryAsync();

        Assert.False(result.Success);
        Assert.Contains("failed", result.Message.ToLower());

        var syncLogs = await context.ErpSyncLogs.ToListAsync();
        Assert.Single(syncLogs);
        Assert.Equal("Failed", syncLogs[0].Status);

        await context.Database.EnsureDeletedAsync();
    }
}
