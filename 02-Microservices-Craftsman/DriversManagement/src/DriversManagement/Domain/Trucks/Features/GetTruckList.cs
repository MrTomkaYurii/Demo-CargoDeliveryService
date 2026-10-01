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

public static class GetTruckList
{
    public sealed record Query(TruckParametersDto QueryParameters) : IRequest<PagedList<TruckDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<TruckDto>>
    {
        private readonly ITruckRepository _truckRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ITruckRepository truckRepository, IHeimGuardClient heimGuard)
        {
            _truckRepository = truckRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<TruckDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadTrucks);

            var collection = _truckRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToTruckDtoQueryable();

            return await PagedList<TruckDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}