namespace CargoOrderingService.Domain.Deliveries.Features;

using CargoOrderingService.Domain.Deliveries.Dtos;
using CargoOrderingService.Domain.Deliveries.Services;
using CargoOrderingService.Exceptions;
using CargoOrderingService.Resources;
using CargoOrderingService.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QueryKit;
using QueryKit.Configuration;

public static class GetAllDeliveries
{
    public sealed record Query() : IRequest<List<DeliveryDto>>;

    public sealed class Handler : IRequestHandler<Query, List<DeliveryDto>>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IDeliveryRepository deliveryRepository, IHeimGuardClient heimGuard)
        {
            _deliveryRepository = deliveryRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<DeliveryDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadDeliveries);

            return _deliveryRepository.Query()
                .AsNoTracking()
                .ToDeliveryDtoQueryable()
                .ToList();
        }
    }
}