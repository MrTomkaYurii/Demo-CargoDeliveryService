namespace DriversManagement.Domain.Expences.Features;

using DriversManagement.Domain.Expences.Dtos;
using DriversManagement.Domain.Expences.Services;
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

public static class GetExpenceList
{
    public sealed record Query(ExpenceParametersDto QueryParameters) : IRequest<PagedList<ExpenceDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<ExpenceDto>>
    {
        private readonly IExpenceRepository _expenceRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IExpenceRepository expenceRepository, IHeimGuardClient heimGuard)
        {
            _expenceRepository = expenceRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<ExpenceDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadExpences);

            var collection = _expenceRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToExpenceDtoQueryable();

            return await PagedList<ExpenceDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}