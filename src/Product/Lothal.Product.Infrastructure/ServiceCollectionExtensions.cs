using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Lothal.Product.Application.Interfaces;
using Lothal.Product.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lothal.Product.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var elasticUrl = configuration["ElasticSearch:Url"] ?? "http://elasticsearch:9200";
        var settings = new ElasticsearchClientSettings(new Uri(elasticUrl))
            .DefaultIndex("products");

        services.AddSingleton(new ElasticsearchClient(settings));
        services.AddScoped<IProductRepository, ElasticSearchProductRepository>();

        services.AddHostedService<ProductSeederService>();

        return services;
    }
}
