using MediatR;

namespace Lothal.Product.Application.Queries;

public record GetProductByBarcodeQuery(string Barcode) : IRequest<Lothal.Product.Domain.Entities.Product?>;
