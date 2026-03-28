using MediatR;

namespace Lothal.Product.Application.Commands;

public record DeleteProductCommand(string Barcode) : IRequest<bool>;
