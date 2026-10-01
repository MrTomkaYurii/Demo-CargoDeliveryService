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

public static class GetAllPhotos
{
    public sealed record Query() : IRequest<List<PhotoDto>>;

    public sealed class Handler : IRequestHandler<Query, List<PhotoDto>>
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IPhotoRepository photoRepository, IHeimGuardClient heimGuard)
        {
            _photoRepository = photoRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<PhotoDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadPhotos);

            return _photoRepository.Query()
                .AsNoTracking()
                .ToPhotoDtoQueryable()
                .ToList();
        }
    }
}