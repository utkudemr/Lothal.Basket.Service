using MediatR;
using Lothal.Basket.Domain.Entities;
using Lothal.Basket.Domain.Repositories;

namespace Lothal.Basket.Application.Queries;

public class GetBasketByIdQueryHandler : IRequestHandler<GetBasketByIdQuery, Domain.Entities.Basket?>
{
    private readonly IBasketRepository _basketRepository;

    public GetBasketByIdQueryHandler(IBasketRepository basketRepository)
    {
        _basketRepository = basketRepository;
    }

    public async Task<Domain.Entities.Basket?> Handle(GetBasketByIdQuery request, CancellationToken cancellationToken)
    {
        return await _basketRepository.GetByIdAsync(request.Id, cancellationToken);
    }
}
