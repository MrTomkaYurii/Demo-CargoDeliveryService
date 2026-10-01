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
using MassTransit;
using SharedKernel.Messages;

public static class AddOrder
{
    public sealed record Command(OrderForCreationDto OrderToAdd) : IRequest<OrderDto>;

    public sealed class Handler : IRequestHandler<Command, OrderDto>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;
        private readonly IPublishEndpoint _publishEndpoint;

        public Handler(
            IOrderRepository orderRepository, 
            IUnitOfWork unitOfWork, 
            IHeimGuardClient heimGuard,
            IPublishEndpoint publishEndpoint)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<OrderDto> Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddOrder);

            var orderToAdd = request.OrderToAdd.ToOrderForCreation();
            var order = Order.Create(orderToAdd);

            await _orderRepository.Add(order, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            // ПАТЕРН EVENT-DRIVEN PUBLISHING (MassTransit / RabbitMQ):
            // Сповіщаємо екосистему мікросервісів про створення нового замовлення на доставку
            await _publishEndpoint.Publish<IOrderCreated>(new OrderCreatedMessage
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                CustomerName = order.CustomerName,
                TotalAmount = order.TotalAmount
            }, cancellationToken);

            return order.ToOrderDto();
        }
    }
}
