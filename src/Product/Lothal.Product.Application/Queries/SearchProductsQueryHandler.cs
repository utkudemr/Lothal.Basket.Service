using Lothal.Product.Application.Interfaces;
using MediatR;
using ProductEntity = Lothal.Product.Domain.Entities.Product;

namespace Lothal.Product.Application.Queries;

public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, IEnumerable<ProductEntity>>
{
    private readonly IProductAutocompleteService _autocompleteService;

    public SearchProductsQueryHandler(IProductAutocompleteService autocompleteService)
    {
        _autocompleteService = autocompleteService;
    }

    public async Task<IEnumerable<ProductEntity>> Handle(
        SearchProductsQuery request, CancellationToken cancellationToken)
    {
        return await _autocompleteService.SearchAsync(request.Q, request.Size);
    }
}
