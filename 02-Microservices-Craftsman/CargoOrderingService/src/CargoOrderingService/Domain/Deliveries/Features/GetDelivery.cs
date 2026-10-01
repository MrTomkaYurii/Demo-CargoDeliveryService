namespace CargoOrderingService.Domain.Deliveries.Features;

using CargoOrderingService.Domain.Deliveries.Dtos;
using CargoOrderingService.Domain.Deliveries.Services;
using CargoOrderingService.Exceptions;
using CargoOrderingService.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetDelivery
{
    public sealed record Query(Guid DeliveryId) : IRequest<DeliveryDto>;

    public sealed class Handler : IRequestHandler<Query, DeliveryDto>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IDeliveryRepository deliveryRepository, IHeimGuardClient heimGuard)
        {
            _deliveryRepository = deliveryRepository;
            _heimGuard = heimGuard;
        }

        public async Task<DeliveryDto> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadDelivery);

            var result = await _deliveryRepository.GetById(request.DeliveryId, cancellationToken: cancellationToken);
            return result.ToDeliveryDto();
        }
    }
}