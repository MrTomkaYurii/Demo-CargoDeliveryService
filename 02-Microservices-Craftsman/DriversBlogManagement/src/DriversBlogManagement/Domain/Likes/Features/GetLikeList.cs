namespace DriversBlogManagement.Domain.Likes.Features;

using DriversBlogManagement.Domain.Likes.Dtos;
using DriversBlogManagement.Domain.Likes.Services;
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

public static class GetLikeList
{
    public sealed record Query(LikeParametersDto QueryParameters) : IRequest<PagedList<LikeDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<LikeDto>>
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ILikeRepository likeRepository, IHeimGuardClient heimGuard)
        {
            _likeRepository = likeRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<LikeDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadLikes);

            var collection = _likeRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToLikeDtoQueryable();

            return await PagedList<LikeDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}