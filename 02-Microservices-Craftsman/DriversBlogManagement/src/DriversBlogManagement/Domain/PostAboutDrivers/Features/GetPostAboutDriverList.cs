namespace DriversBlogManagement.Domain.PostAboutDrivers.Features;

using DriversBlogManagement.Domain.PostAboutDrivers.Dtos;
using DriversBlogManagement.Domain.PostAboutDrivers.Services;
using DriversBlogManagement.Wrappers;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Resources;
using DriversBlogManagement.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QueryKit;
using QueryKit.Configuration;

public static class GetPostAboutDriverList
{
    public sealed record Query(PostAboutDriverParametersDto QueryParameters) : IRequest<PagedList<PostAboutDriverDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<PostAboutDriverDto>>
    {
        private readonly IPostAboutDriverRepository _postAboutDriverRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IPostAboutDriverRepository postAboutDriverRepository, IHeimGuardClient heimGuard)
        {
            _postAboutDriverRepository = postAboutDriverRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<PostAboutDriverDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadPostAboutDrivers);

            var collection = _postAboutDriverRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToPostAboutDriverDtoQueryable();

            return await PagedList<PostAboutDriverDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}