namespace CargoOrderingService.Domain.Orders.Features;

using CargoOrderingService.Domain.Orders.Dtos;
using CargoOrderingService.Domain.Orders.Services;
using CargoOrderingService.Exceptions;
using CargoOrderingService.Resources;
using CargoOrderingService.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QueryKit;
using QueryKit.Configuration;

public static class GetAllOrders
{
    public sealed record Query() : IRequest<List<OrderDto>>;

    public sealed class Handler : IRequestHandler<Query, List<OrderDto>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IOrderRepository orderRepository, IHeimGuardClient heimGuard)
        {
            _orderRepository = orderRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<OrderDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadOrders);

            return _orderRepository.Query()
                .AsNoTracking()
                .ToOrderDtoQueryable()
                .ToList();
        }
    }
}