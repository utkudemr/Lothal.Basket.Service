using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Lothal.Product.Application.Interfaces;
using Lothal.Product.Domain.Entities;
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

    public async Task<Lothal.Product.Domain.Entities.Product?> GetByBarcodeAsync(string barcode)
    {
        var response = await _client.GetAsync<Lothal.Product.Domain.Entities.Product>(barcode, idx => idx.Index(IndexName));
        if (response.IsValidResponse && response.Found)
        {
            return response.Source;
        }
        
        return null;
    }

    public async Task<IEnumerable<Lothal.Product.Domain.Entities.Product>> GetAllAsync(int from, int size)
    {
        var response = await _client.SearchAsync<Lothal.Product.Domain.Entities.Product>(s => s
            .Index(IndexName)
            .From(from)
            .Size(size)
            .Query(q => q.MatchAll(_ => { }))
        );

        if (response.IsValidResponse)
        {
            return response.Documents;
        }

        _logger.LogError("Error fetching all products: {Error}", response.DebugInformation);
        return Enumerable.Empty<Lothal.Product.Domain.Entities.Product>();
    }

    public async Task<bool> DeleteAsync(string barcode)
    {
        var response = await _client.DeleteAsync<Lothal.Product.Domain.Entities.Product>(barcode, idx => idx.Index(IndexName));
        
        if (!response.IsValidResponse)
        {
            _logger.LogError("Error deleting product {Barcode}: {Error}", barcode, response.DebugInformation);
            return false;
        }

        return response.Result == Result.Deleted || response.Result == Result.NotFound;
    }

    public async Task BulkMergeAsync(IEnumerable<Lothal.Product.Domain.Entities.Product> products)
    {
        var bulkRequest = new BulkRequest(IndexName)
        {
            Operations = new BulkOperationsCollection()
        };

        foreach (var product in products)
        {
            // use barcode as the document ID for deduplication and direct gets
            product.Id = product.Barcode;
            bulkRequest.Operations.Add(new BulkIndexOperation<Lothal.Product.Domain.Entities.Product>(product) { Id = product.Barcode });
        }

        var response = await _client.BulkAsync(bulkRequest);
        if (!response.IsValidResponse)
        {
            _logger.LogError("Error in bulk merge: {Error}", response.DebugInformation);
        }
    }

    public async Task SeedDataAsync(IEnumerable<Lothal.Product.Domain.Entities.Product> products)
    {
        var existsResponse = await _client.Indices.ExistsAsync(IndexName);
        if (!existsResponse.Exists)
        {
            _logger.LogInformation("Products index does not exist, creating and seeding.");
            var createResponse = await _client.Indices.CreateAsync(IndexName, c => c
                .Mappings(m => m
                    .Properties<Lothal.Product.Domain.Entities.Product>(p => p
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
