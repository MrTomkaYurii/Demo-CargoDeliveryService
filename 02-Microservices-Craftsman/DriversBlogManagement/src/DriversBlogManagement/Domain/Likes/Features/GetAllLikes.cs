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

public static class GetAllLikes
{
    public sealed record Query() : IRequest<List<LikeDto>>;

    public sealed class Handler : IRequestHandler<Query, List<LikeDto>>
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ILikeRepository likeRepository, IHeimGuardClient heimGuard)
        {
            _likeRepository = likeRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<LikeDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadLikes);

            return _likeRepository.Query()
                .AsNoTracking()
                .ToLikeDtoQueryable()
                .ToList();
        }
    }
}