using Lothal.Product.Application.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lothal.BuildingBlocks.Common;
using ProductEntity = Lothal.Product.Domain.Entities.Product;

namespace Lothal.Product.Application.Queries;

public class GetAllProductsQueryHandler
    : IRequestHandler<GetAllProductsQuery, PagedResult<ProductEntity>>
{
    private readonly IProductRepository _repository;

    public GetAllProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ProductEntity>> Handle(
        GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(request.From, request.Size);
    }
}
