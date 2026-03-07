using Lothal.Basket.Service.Application;
using Lothal.Basket.Service.Application.Commands;
using Lothal.Basket.Service.Infrastructure;
using Lothal.Basket.Service.Infrastructure.Data;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Layered Dependencies
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Apply DB migrations on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
}

// Map Minimal API Endpoints
app.MapPost("/api/baskets", async (CreateBasketCommand command, IMediator mediator) =>
{
    var basketId = await mediator.Send(command);
    return Results.Created($"/api/baskets/{basketId}", new { Id = basketId });
})
.WithName("CreateBasket")
.WithOpenApi();

app.Run();
