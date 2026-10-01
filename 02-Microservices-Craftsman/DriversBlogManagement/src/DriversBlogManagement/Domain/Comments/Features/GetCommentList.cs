namespace DriversBlogManagement.Domain.Comments.Features;

using DriversBlogManagement.Domain.Comments.Dtos;
using DriversBlogManagement.Domain.Comments.Services;
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

public static class GetCommentList
{
    public sealed record Query(CommentParametersDto QueryParameters) : IRequest<PagedList<CommentDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<CommentDto>>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ICommentRepository commentRepository, IHeimGuardClient heimGuard)
        {
            _commentRepository = commentRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<CommentDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadComments);

            var collection = _commentRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToCommentDtoQueryable();

            return await PagedList<CommentDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}