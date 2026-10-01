namespace DriversManagement.Domain.Photos.Features;

using DriversManagement.Domain.Photos.Dtos;
using DriversManagement.Domain.Photos.Services;
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

public static class GetPhotoList
{
    public sealed record Query(PhotoParametersDto QueryParameters) : IRequest<PagedList<PhotoDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<PhotoDto>>
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IPhotoRepository photoRepository, IHeimGuardClient heimGuard)
        {
            _photoRepository = photoRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<PhotoDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadPhotos);

            var collection = _photoRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToPhotoDtoQueryable();

            return await PagedList<PhotoDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}