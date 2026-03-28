using Lothal.Product.Application.Interfaces;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lothal.Product.Application.Queries;

public class GetAllProductsQueryHandler
    : IRequestHandler<GetAllProductsQuery, IEnumerable<Lothal.Product.Domain.Entities.Product>>
{
    private readonly IProductRepository _repository;

    public GetAllProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Lothal.Product.Domain.Entities.Product>> Handle(
        GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(request.From, request.Size);
    }
}
