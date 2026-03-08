var builder = WebApplication.CreateBuilder(args);

// Add the reverse proxy to capability to the server
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapReverseProxy();

app.Run();
