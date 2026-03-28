using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Lothal.Product.Application.Interfaces;
using ProductEntity = Lothal.Product.Domain.Entities.Product;
using Lothal.BuildingBlocks.Common;
using Microsoft.Extensions.Logging;

namespace Lothal.Product.Infrastructure.Data;

public class ElasticSearchProductRepository : IProductRepository
{
    private readonly ElasticsearchClient _client;
    private readonly ILogger<ElasticSearchProductRepository> _logger;
    private const string IndexName = "products";

    public ElasticSearchProductRepository(ElasticsearchClient client, ILogger<ElasticSearchProductRepository> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<ProductEntity?> GetByBarcodeAsync(string barcode)
    {
        var response = await _client.GetAsync<ProductEntity>(barcode, idx => idx.Index(IndexName));
        if (response.IsValidResponse && response.Found)
        {
            return response.Source;
        }
        
        return null;
    }

    public async Task<PagedResult<ProductEntity>> GetAllAsync(int from, int size)
    {
        var response = await _client.SearchAsync<ProductEntity>(s => s
            .Index(IndexName)
            .From(from)
            .Size(size)
            .Sort(srt => srt.Field(f => f.Barcode, d => d.Order(SortOrder.Asc)))
            .Query(q => q.MatchAll(_ => { }))
        );

        if (response.IsValidResponse)
        {
            return new PagedResult<ProductEntity>(
                response.Documents, 
                (int)response.Total);
        }

        _logger.LogError("Error fetching all products: {Error}", response.DebugInformation);
        return new PagedResult<ProductEntity>(
            Enumerable.Empty<ProductEntity>(), 
            0);
    }

    public async Task<bool> DeleteAsync(string barcode)
    {
        var response = await _client.DeleteAsync<ProductEntity>(barcode, idx => idx.Index(IndexName));
        
        if (!response.IsValidResponse)
        {
            _logger.LogError("Error deleting product {Barcode}: {Error}", barcode, response.DebugInformation);
            return false;
        }

        return response.Result == Result.Deleted || response.Result == Result.NotFound;
    }

    public async Task BulkMergeAsync(IEnumerable<ProductEntity> products)
    {
        var bulkRequest = new BulkRequest(IndexName)
        {
            Operations = new BulkOperationsCollection()
        };

        foreach (var product in products)
        {
            // use barcode as the document ID for deduplication and direct gets
            product.Id = product.Barcode;
            bulkRequest.Operations.Add(new BulkIndexOperation<ProductEntity>(product) { Id = product.Barcode });
        }

        var response = await _client.BulkAsync(bulkRequest);
        if (!response.IsValidResponse)
        {
            _logger.LogError("Error in bulk merge: {Error}", response.DebugInformation);
        }
    }

    public async Task SeedDataAsync(IEnumerable<ProductEntity> products)
    {
        var existsResponse = await _client.Indices.ExistsAsync(IndexName);
        if (!existsResponse.Exists)
        {
            _logger.LogInformation("Products index does not exist, creating and seeding.");
            var createResponse = await _client.Indices.CreateAsync(IndexName, c => c
                .Mappings(m => m
                    .Properties<ProductEntity>(p => p
                        .Keyword(k => k.Barcode)
                        .Text(t => t.Name)
                        .Keyword(k => k.Class)
                        .Keyword(k => k.Color)
                        .Keyword(k => k.Size)
                    )
                )
            );

            if (createResponse.IsValidResponse)
            {
                await BulkMergeAsync(products);
            }
            else
            {
                _logger.LogError("Failed to create products index: {Debug}", createResponse.DebugInformation);
            }
        }
    }
}

