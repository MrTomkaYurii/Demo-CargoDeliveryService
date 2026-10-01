namespace DriversBlogManagement.Domain.BlogUsers.Features;

using DriversBlogManagement.Domain.BlogUsers.Dtos;
using DriversBlogManagement.Domain.BlogUsers.Services;
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

public static class GetBlogUserList
{
    public sealed record Query(BlogUserParametersDto QueryParameters) : IRequest<PagedList<BlogUserDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<BlogUserDto>>
    {
        private readonly IBlogUserRepository _blogUserRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IBlogUserRepository blogUserRepository, IHeimGuardClient heimGuard)
        {
            _blogUserRepository = blogUserRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<BlogUserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadUsers);

            var collection = _blogUserRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToBlogUserDtoQueryable();

            return await PagedList<BlogUserDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}