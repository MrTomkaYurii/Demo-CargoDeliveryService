namespace DriversManagement.Domain.Drivers.Features;

using DriversManagement.Domain.Drivers.Dtos;
using DriversManagement.Domain.Drivers.Services;
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

public static class GetAllDrivers
{
    public sealed record Query() : IRequest<List<DriverDto>>;

    public sealed class Handler : IRequestHandler<Query, List<DriverDto>>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IDriverRepository driverRepository, IHeimGuardClient heimGuard)
        {
            _driverRepository = driverRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<DriverDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadDrivers);

            return _driverRepository.Query()
                .AsNoTracking()
                .ToDriverDtoQueryable()
                .ToList();
        }
    }
}