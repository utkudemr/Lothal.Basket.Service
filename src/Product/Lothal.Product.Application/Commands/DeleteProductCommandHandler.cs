using Lothal.Product.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Lothal.Product.Application.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _repository;
    private readonly IProductAutocompleteService _autocompleteService;

    public DeleteProductCommandHandler(
        IProductRepository repository, 
        IProductAutocompleteService autocompleteService)
    {
        _repository = repository;
        _autocompleteService = autocompleteService;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.DeleteAsync(request.Barcode);
        if (result)
        {
            await _autocompleteService.DeleteProductAsync(request.Barcode);
        }
        return result;
    }
}
