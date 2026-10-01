namespace DriversManagement.Domain.Trucks.Features;

using DriversManagement.Domain.Trucks.Dtos;
using DriversManagement.Domain.Trucks.Services;
using DriversManagement.Wrappers;
using DriversManagement.Exceptions;
using DriversManagement.Resources;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QueryKit;
using QueryKit.Configuration;

public static class GetAllTrucks
{
    public sealed record Query() : IRequest<List<TruckDto>>;

    public sealed class Handler : IRequestHandler<Query, List<TruckDto>>
    {
        private readonly ITruckRepository _truckRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ITruckRepository truckRepository, IHeimGuardClient heimGuard)
        {
            _truckRepository = truckRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<TruckDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadTrucks);

            return _truckRepository.Query()
                .AsNoTracking()
                .ToTruckDtoQueryable()
                .ToList();
        }
    }
}