namespace DriversManagement.Domain.Photos.Features;

using DriversManagement.Domain.Photos.Services;
using DriversManagement.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using MediatR;

public static class DeletePhoto
{
    public sealed record Command(Guid PhotoId) : IRequest;

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
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeletePhoto);

            var recordToDelete = await _photoRepository.GetById(request.PhotoId, cancellationToken: cancellationToken);
            _photoRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}