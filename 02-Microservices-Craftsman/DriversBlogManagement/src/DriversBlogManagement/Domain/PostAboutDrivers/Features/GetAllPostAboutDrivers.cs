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

public static class GetAllPostAboutDrivers
{
    public sealed record Query() : IRequest<List<PostAboutDriverDto>>;

    public sealed class Handler : IRequestHandler<Query, List<PostAboutDriverDto>>
    {
        private readonly IPostAboutDriverRepository _postAboutDriverRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IPostAboutDriverRepository postAboutDriverRepository, IHeimGuardClient heimGuard)
        {
            _postAboutDriverRepository = postAboutDriverRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<PostAboutDriverDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadPostAboutDrivers);

            return _postAboutDriverRepository.Query()
                .AsNoTracking()
                .ToPostAboutDriverDtoQueryable()
                .ToList();
        }
    }
}