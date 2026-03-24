using Lothal.Stock.Application.Interfaces;
using Lothal.Stock.Infrastructure.Data;
using Lothal.Stock.Infrastructure.Messaging;
using Lothal.Stock.Infrastructure.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using StackExchange.Redis;

namespace Lothal.Stock.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStockInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("StockDb")
            ?? "Host=basket-db;Database=StockDb;Username=postgres;Password=postgres";

        // Scoped repository — opens a fresh connection per request
        services.AddScoped<IStockRepository, PostgresStockRepository>();

        // Redis — reuse the same Redis instance as basket-api
        var redisConnection = configuration.GetConnectionString("Redis") ?? "redis:6379";
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnection));

        services.AddScoped<IStockReservationService, RedisStockReservationService>();

        // NATS hosted service — subscribes to "stock.upsert"
        services.AddHostedService<StockNatsConsumer>();

        // Ensure PostgreSQL table exists (lightweight schema bootstrap — replace with Flyway/EF in prod)
        EnsureSchema(connectionString);

        return services;
    }

    private static void EnsureSchema(string connectionString)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        const string ddl = """
            CREATE TABLE IF NOT EXISTS stocks (
                barcode             TEXT PRIMARY KEY,
                warehouse_quantity  INTEGER NOT NULL DEFAULT 0,
                source              TEXT NOT NULL DEFAULT 'UNKNOWN',
                last_updated_at     TIMESTAMPTZ NOT NULL DEFAULT NOW()
            )
            """;

        using var cmd = new NpgsqlCommand(ddl, conn);
        cmd.ExecuteNonQuery();
    }
}
