namespace CargoOrderingService.Domain.Deliveries.Features;

using CargoOrderingService.Domain.Deliveries.Services;
using CargoOrderingService.Domain.Deliveries;
using CargoOrderingService.Domain.Deliveries.Dtos;
using CargoOrderingService.Domain.Deliveries.Models;
using CargoOrderingService.Services;
using CargoOrderingService.Exceptions;
using CargoOrderingService.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddDelivery
{
    public sealed record Command(DeliveryForCreationDto DeliveryToAdd) : IRequest<DeliveryDto>;

    public sealed class Handler : IRequestHandler<Command, DeliveryDto>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IDeliveryRepository deliveryRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _deliveryRepository = deliveryRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task<DeliveryDto> Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddDelivery);

            var deliveryToAdd = request.DeliveryToAdd.ToDeliveryForCreation();
            var delivery = Delivery.Create(deliveryToAdd);

            await _deliveryRepository.Add(delivery, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return delivery.ToDeliveryDto();
        }
    }
}