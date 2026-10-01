namespace CargoOrderingService.Domain.Orders.Features;

using CargoOrderingService.Domain.Orders.Dtos;
using CargoOrderingService.Domain.Orders.Services;
using CargoOrderingService.Exceptions;
using CargoOrderingService.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetOrder
{
    public sealed record Query(Guid OrderId) : IRequest<OrderDto>;

    public sealed class Handler : IRequestHandler<Query, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IOrderRepository orderRepository, IHeimGuardClient heimGuard)
        {
            _orderRepository = orderRepository;
            _heimGuard = heimGuard;
        }

        public async Task<OrderDto> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadOrder);

            var result = await _orderRepository.GetById(request.OrderId, cancellationToken: cancellationToken);
            return result.ToOrderDto();
        }
    }
}