namespace DriversManagement.Domain.Photos.Features;

using DriversManagement.Domain.Photos.Services;
using DriversManagement.Domain.Photos;
using DriversManagement.Domain.Photos.Dtos;
using DriversManagement.Domain.Photos.Models;
using DriversManagement.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddPhoto
{
    public sealed record Command(PhotoForCreationDto PhotoToAdd) : IRequest<PhotoDto>;

    public sealed class Handler : IRequestHandler<Command, PhotoDto>
    {
        private readonly IPhotoRepository _photoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IPhotoRepository photoRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _photoRepository = photoRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task<PhotoDto> Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddPhoto);

            var photoToAdd = request.PhotoToAdd.ToPhotoForCreation();
            var photo = Photo.Create(photoToAdd);

            await _photoRepository.Add(photo, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return photo.ToPhotoDto();
        }
    }
}