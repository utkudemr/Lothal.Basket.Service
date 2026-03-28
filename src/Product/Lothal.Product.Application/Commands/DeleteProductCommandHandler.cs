using Lothal.Product.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Lothal.Product.Application.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _repository;

    public DeleteProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DeleteAsync(request.Barcode);
    }
}
