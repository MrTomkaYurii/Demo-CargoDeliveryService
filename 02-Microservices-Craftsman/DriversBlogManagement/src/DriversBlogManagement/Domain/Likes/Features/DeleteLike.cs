namespace DriversBlogManagement.Domain.Likes.Features;

using DriversBlogManagement.Domain.Likes.Services;
using DriversBlogManagement.Services;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Domain;
using HeimGuard;
using MediatR;

public static class DeleteLike
{
    public sealed record Command(Guid LikeId) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ILikeRepository likeRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _likeRepository = likeRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeleteLike);

            var recordToDelete = await _likeRepository.GetById(request.LikeId, cancellationToken: cancellationToken);
            _likeRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}