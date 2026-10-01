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

public static class GetRepairList
{
    public sealed record Query(RepairParametersDto QueryParameters) : IRequest<PagedList<RepairDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<RepairDto>>
    {
        private readonly IRepairRepository _repairRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IRepairRepository repairRepository, IHeimGuardClient heimGuard)
        {
            _repairRepository = repairRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<RepairDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadRepairs);

            var collection = _repairRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToRepairDtoQueryable();

            return await PagedList<RepairDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}