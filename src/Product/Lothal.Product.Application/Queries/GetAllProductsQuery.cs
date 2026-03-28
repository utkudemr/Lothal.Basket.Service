using MediatR;
using Lothal.BuildingBlocks.Common;
using ProductEntity = Lothal.Product.Domain.Entities.Product;

namespace Lothal.Product.Application.Queries;

public record GetAllProductsQuery(int From = 0, int Size = 100) : IRequest<PagedResult<ProductEntity>>;
