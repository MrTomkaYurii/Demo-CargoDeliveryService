namespace CargoOrderingService.Domain.Orders.Features;

using CargoOrderingService.Domain.Orders.Services;
using CargoOrderingService.Domain.Orders;
using CargoOrderingService.Domain.Orders.Dtos;
using CargoOrderingService.Domain.Orders.Models;
using CargoOrderingService.Services;
using CargoOrderingService.Exceptions;
using CargoOrderingService.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddOrder
{
    public sealed record Command(OrderForCreationDto OrderToAdd) : IRequest<OrderDto>;

    public sealed class Handler : IRequestHandler<Command, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task<OrderDto> Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddOrder);

            var orderToAdd = request.OrderToAdd.ToOrderForCreation();
            var order = Order.Create(orderToAdd);

            await _orderRepository.Add(order, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return order.ToOrderDto();
        }
    }
}