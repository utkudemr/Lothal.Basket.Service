using System.Text.Json;
using Lothal.Product.Application.Interfaces;
using StackExchange.Redis;
using ProductEntity = Lothal.Product.Domain.Entities.Product;

namespace Lothal.Product.Infrastructure.Data;

public class RedisAutocompleteService : IProductAutocompleteService
{
    private readonly IConnectionMultiplexer _redis;
    private const string AutocompleteKey = "product:autocomplete";
    private const string DataKey = "products:data";

    public RedisAutocompleteService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task IndexProductsAsync(IEnumerable<ProductEntity> products)
    {
        var db = _redis.GetDatabase();
        var batch = db.CreateBatch();
        var tasks = new List<Task>();

        foreach (var product in products)
        {
            var json = JsonSerializer.Serialize(product);
            tasks.Add(batch.HashSetAsync(DataKey, product.Barcode, json));

            var tokens = GenerateSearchTokens(product.Name, product.Barcode);
            foreach (var token in tokens)
            {
                tasks.Add(batch.SortedSetAddAsync(AutocompleteKey, token, 0));
            }
        }

        batch.Execute();
        await Task.WhenAll(tasks);
    }

    public async Task DeleteProductAsync(string barcode)
    {
        var db = _redis.GetDatabase();

        // Data dictionary'den sil
        await db.HashDeleteAsync(DataKey, barcode);

        // Sorted set'ten bu barcode'a ait tüm kayıtları silmek için bir pattern veya eşleştirme lazım.
        // Ama Redis'te değere göre arama ZSCAN ile mümkündür ve o da yavaştır. 
        // Ancak bizim sistemde ürün silinmesi çok nadir, ZSCAN ile temizleyebiliriz
        // veya ismi bilmiyorsak sadece hash'ten silecek, ZRANGE sorgusunda eksik JSON gelirse eler.
        // En pratiği lazy-deletion: Hash'ten sildik, Sorted Set'te kalsa bile Data dictionary'de 
        // JSON'u olmadığı için UI'a dönmeyecek.
    }

    public async Task<IEnumerable<ProductEntity>> SearchAsync(string query, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<ProductEntity>();

        var db = _redis.GetDatabase();
        var normalizedQuery = query.ToLowerInvariant().Trim();

        var lexResult = await db.SortedSetRangeByValueAsync(
            AutocompleteKey,
            min: normalizedQuery,
            max: normalizedQuery + "\xff",
            take: 50 // Biraz fazlasını alıp benzersizleri filtreleyeceğiz
        );

        var barcodes = lexResult
            .Select(x => x.ToString().Split(':').LastOrDefault())
            .Where(b => !string.IsNullOrEmpty(b))
            .Distinct()
            .Take(maxResults)
            .Select(b => (RedisValue)b!)
            .ToArray();

        if (barcodes.Length == 0)
            return Array.Empty<ProductEntity>();

        // Hash'ten orijinal ürün verilerini çek
        var jsonResults = await db.HashGetAsync(DataKey, barcodes);

        return jsonResults
            .Where(j => j.HasValue)
            .Select(j => JsonSerializer.Deserialize<ProductEntity>(j.ToString()))
            .Where(p => p != null)!;
    }

    private IEnumerable<string> GenerateSearchTokens(string name, string barcode)
    {
        name = name.ToLowerInvariant().Trim();
        var list = new HashSet<string> { $"{name}:{barcode}" };
        
        var words = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        // Tüm suffix setlerini oluştur. "siyah kisa t-shirt" -> "kisa t-shirt", "t-shirt"
        for (int i = 1; i < words.Length; i++)
        {
            var suffix = string.Join(" ", words.Skip(i));
            list.Add($"{suffix}:{barcode}");
        }

        return list;
    }
}
