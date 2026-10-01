namespace DriversBlogManagement.Domain.PostAboutDrivers.Features;

using DriversBlogManagement.Domain.PostAboutDrivers.Dtos;
using DriversBlogManagement.Domain.PostAboutDrivers.Services;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetPostAboutDriver
{
    public sealed record Query(Guid PostAboutDriverId) : IRequest<PostAboutDriverDto>;

    public sealed class Handler : IRequestHandler<Query, PostAboutDriverDto>
    {
        private readonly IPostAboutDriverRepository _postAboutDriverRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IPostAboutDriverRepository postAboutDriverRepository, IHeimGuardClient heimGuard)
        {
            _postAboutDriverRepository = postAboutDriverRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PostAboutDriverDto> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadPostAboutDriver);

            var result = await _postAboutDriverRepository.GetById(request.PostAboutDriverId, cancellationToken: cancellationToken);
            return result.ToPostAboutDriverDto();
        }
    }
}