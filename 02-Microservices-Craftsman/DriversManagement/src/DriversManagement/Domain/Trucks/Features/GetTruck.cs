namespace DriversManagement.Domain.Trucks.Features;

using DriversManagement.Domain.Trucks.Dtos;
using DriversManagement.Domain.Trucks.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetTruck
{
    public sealed record Query(Guid TruckId) : IRequest<TruckDto>;

    public sealed class Handler : IRequestHandler<Query, TruckDto>
    {
        private readonly ITruckRepository _truckRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ITruckRepository truckRepository, IHeimGuardClient heimGuard)
        {
            _truckRepository = truckRepository;
            _heimGuard = heimGuard;
        }

        public async Task<TruckDto> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadTruck);

            var result = await _truckRepository.GetById(request.TruckId, cancellationToken: cancellationToken);
            return result.ToTruckDto();
        }
    }
}