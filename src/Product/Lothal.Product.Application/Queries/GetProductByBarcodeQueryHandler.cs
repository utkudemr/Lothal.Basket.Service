using Lothal.Product.Application.Interfaces;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Lothal.Product.Application.Queries;

public class GetProductByBarcodeQueryHandler : IRequestHandler<GetProductByBarcodeQuery, Lothal.Product.Domain.Entities.Product?>
{
    private readonly IProductRepository _repository;

    public GetProductByBarcodeQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Lothal.Product.Domain.Entities.Product?> Handle(GetProductByBarcodeQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByBarcodeAsync(request.Barcode);
    }
}
