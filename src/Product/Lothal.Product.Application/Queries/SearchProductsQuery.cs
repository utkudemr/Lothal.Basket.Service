using MediatR;
using ProductEntity = Lothal.Product.Domain.Entities.Product;

namespace Lothal.Product.Application.Queries;

public record SearchProductsQuery(string Q, int Size = 10) : IRequest<IEnumerable<ProductEntity>>;
