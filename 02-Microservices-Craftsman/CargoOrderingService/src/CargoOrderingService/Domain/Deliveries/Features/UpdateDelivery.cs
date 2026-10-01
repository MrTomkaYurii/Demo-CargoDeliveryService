namespace CargoOrderingService.Domain.Deliveries.Features;

using CargoOrderingService.Domain.Deliveries;
using CargoOrderingService.Domain.Deliveries.Dtos;
using CargoOrderingService.Domain.Deliveries.Services;
using CargoOrderingService.Services;
using CargoOrderingService.Domain.Deliveries.Models;
using CargoOrderingService.Exceptions;
using CargoOrderingService.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateDelivery
{
    public sealed record Command(Guid DeliveryId, DeliveryForUpdateDto UpdatedDeliveryData) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
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

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateDelivery);

            var deliveryToUpdate = await _deliveryRepository.GetById(request.DeliveryId, cancellationToken: cancellationToken);
            var deliveryToAdd = request.UpdatedDeliveryData.ToDeliveryForUpdate();
            deliveryToUpdate.Update(deliveryToAdd);

            _deliveryRepository.Update(deliveryToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}