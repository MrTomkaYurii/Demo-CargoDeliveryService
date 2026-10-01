namespace CargoOrderingService.Domain.Orders.Features;

using CargoOrderingService.Domain.Orders;
using CargoOrderingService.Domain.Orders.Dtos;
using CargoOrderingService.Domain.Orders.Services;
using CargoOrderingService.Services;
using CargoOrderingService.Domain.Orders.Models;
using CargoOrderingService.Exceptions;
using CargoOrderingService.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateOrder
{
    public sealed record Command(Guid OrderId, OrderForUpdateDto UpdatedOrderData) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
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

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateOrder);

            var orderToUpdate = await _orderRepository.GetById(request.OrderId, cancellationToken: cancellationToken);
            var orderToAdd = request.UpdatedOrderData.ToOrderForUpdate();
            orderToUpdate.Update(orderToAdd);

            _orderRepository.Update(orderToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}