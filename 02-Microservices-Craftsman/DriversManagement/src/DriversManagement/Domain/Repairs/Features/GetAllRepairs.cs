namespace DriversManagement.Domain.Repairs.Features;

using DriversManagement.Domain.Repairs.Dtos;
using DriversManagement.Domain.Repairs.Services;
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

public static class GetAllRepairs
{
    public sealed record Query() : IRequest<List<RepairDto>>;

    public sealed class Handler : IRequestHandler<Query, List<RepairDto>>
    {
        private readonly IRepairRepository _repairRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IRepairRepository repairRepository, IHeimGuardClient heimGuard)
        {
            _repairRepository = repairRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<RepairDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadRepairs);

            return _repairRepository.Query()
                .AsNoTracking()
                .ToRepairDtoQueryable()
                .ToList();
        }
    }
}