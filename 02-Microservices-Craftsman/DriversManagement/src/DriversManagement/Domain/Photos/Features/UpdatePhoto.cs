namespace DriversManagement.Domain.Photos.Features;

using DriversManagement.Domain.Photos;
using DriversManagement.Domain.Photos.Dtos;
using DriversManagement.Domain.Photos.Services;
using DriversManagement.Services;
using DriversManagement.Domain.Photos.Models;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdatePhoto
{
    public sealed record Command(Guid PhotoId, PhotoForUpdateDto UpdatedPhotoData) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
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

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdatePhoto);

            var photoToUpdate = await _photoRepository.GetById(request.PhotoId, cancellationToken: cancellationToken);
            var photoToAdd = request.UpdatedPhotoData.ToPhotoForUpdate();
            photoToUpdate.Update(photoToAdd);

            _photoRepository.Update(photoToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}