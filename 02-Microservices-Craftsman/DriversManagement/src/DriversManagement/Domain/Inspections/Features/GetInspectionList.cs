namespace DriversManagement.Domain.Inspections.Features;

using DriversManagement.Domain.Inspections.Dtos;
using DriversManagement.Domain.Inspections.Services;
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

public static class GetInspectionList
{
    public sealed record Query(InspectionParametersDto QueryParameters) : IRequest<PagedList<InspectionDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<InspectionDto>>
    {
        private readonly IInspectionRepository _inspectionRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IInspectionRepository inspectionRepository, IHeimGuardClient heimGuard)
        {
            _inspectionRepository = inspectionRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<InspectionDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadInspections);

            var collection = _inspectionRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToInspectionDtoQueryable();

            return await PagedList<InspectionDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}