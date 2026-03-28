using MediatR;

namespace Lothal.Product.Application.Queries;

public record GetAllProductsQuery(int From = 0, int Size = 200) : IRequest<IEnumerable<Lothal.Product.Domain.Entities.Product>>;
