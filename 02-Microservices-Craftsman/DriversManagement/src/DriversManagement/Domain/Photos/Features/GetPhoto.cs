namespace DriversManagement.Domain.Photos.Features;

using DriversManagement.Domain.Photos.Dtos;
using DriversManagement.Domain.Photos.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetPhoto
{
    public sealed record Query(Guid PhotoId) : IRequest<PhotoDto>;

    public sealed class Handler : IRequestHandler<Query, PhotoDto>
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IPhotoRepository photoRepository, IHeimGuardClient heimGuard)
        {
            _photoRepository = photoRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PhotoDto> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadPhoto);

            var result = await _photoRepository.GetById(request.PhotoId, cancellationToken: cancellationToken);
            return result.ToPhotoDto();
        }
    }
}