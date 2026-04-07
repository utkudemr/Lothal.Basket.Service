using Lothal.Product.Application.Interfaces;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ProductEntity = Lothal.Product.Domain.Entities.Product;

namespace Lothal.Product.Application.Commands;

public class BulkMergeProductsCommandHandler : IRequestHandler<BulkMergeProductsCommand, bool>
{
    private readonly IProductRepository _repository;
    private readonly IProductAutocompleteService _autocompleteService;

    public BulkMergeProductsCommandHandler(
        IProductRepository repository, 
        IProductAutocompleteService autocompleteService)
    {
        _repository = repository;
        _autocompleteService = autocompleteService;
    }

    public async Task<bool> Handle(BulkMergeProductsCommand request, CancellationToken cancellationToken)
    {
        if (request.Products == null || !request.Products.Any())
            return false;

        var entities = request.Products.Select(p => new ProductEntity
        {
            Barcode = p.Barcode,
            Price = p.Price,
            Name = p.Name,
            Class = p.Class,
            Color = p.Color,
            Size = p.Size
        });

        var entityList = entities.ToList();
        await _repository.BulkMergeAsync(entityList);
        await _autocompleteService.IndexProductsAsync(entityList);
        return true;
    }
}
