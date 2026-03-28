using System.Net.Http.Json;
using System.Text.Json;

var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5024") }; // API Gateway

Console.WriteLine("🚀 Starting Massive Catalog Seeder (1000 items)...");

var products = new List<object>();
var random = new Random();

string[] classes = { "Electronics", "Home", "Clothing", "Toys", "Kitchen" };
string[] colors = { "Black", "White", "Red", "Blue", "Silver", "Green" };
string[] sizes = { "S", "M", "L", "XL", "N/A" };

for (int i = 1; i <= 1000; i++)
{
    var barcode = $"P{1000 + i}";
    products.Add(new
    {
        Barcode = barcode,
        Name = $"Product {i} - Automated",
        Price = Math.Round(random.NextDouble() * 1000 + 10, 2),
        Class = classes[random.Next(classes.Length)],
        Color = colors[random.Next(colors.Length)],
        Size = sizes[random.Next(sizes.Length)]
    });

    if (i % 100 == 0) Console.WriteLine($"Generated {i} products...");
}

Console.WriteLine("📡 Sending bulk merge request to Product API via Gateway...");
var response = await httpClient.PostAsJsonAsync("/api/gateway/products/bulk-merge", new { Products = products });

if (response.IsSuccessStatusCode)
{
    Console.WriteLine("✅ Products successfully merged in Elasticsearch.");
}
else
{
    var error = await response.Content.ReadAsStringAsync();
    Console.WriteLine($"❌ Failed to merge products: {response.StatusCode} - {error}");
    return;
}

Console.WriteLine("🔔 Initializing stock for 1000 products (sending upserts)...");
// We call the Stock API directly or via Gateway. Let's use Gateway.
// In high-traffic simulation, we'd normally use NATS, 
// but for the seeder, a sequence of HTTP calls or a bulk endpoint is fine.
// Let's assume we want to initialize them with 0 stock.

foreach (var p in products)
{
    var barcode = (string)((dynamic)p).Barcode;
    // Calling upsert to initialize the record in Postgres
    await httpClient.PutAsJsonAsync("/api/stocks/upsert", new { Barcode = barcode, Quantity = 0, Source = "SEEDER" });
}

Console.WriteLine("🏁 Seeding complete!");
