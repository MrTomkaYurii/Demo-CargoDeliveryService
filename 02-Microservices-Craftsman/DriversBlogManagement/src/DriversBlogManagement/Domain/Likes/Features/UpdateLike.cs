namespace DriversBlogManagement.Domain.Likes.Features;

using DriversBlogManagement.Domain.Likes;
using DriversBlogManagement.Domain.Likes.Dtos;
using DriversBlogManagement.Domain.Likes.Services;
using DriversBlogManagement.Services;
using DriversBlogManagement.Domain.Likes.Models;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateLike
{
    public sealed record Command(Guid LikeId, LikeForUpdateDto UpdatedLikeData) : IRequest;

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
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateLike);

            var likeToUpdate = await _likeRepository.GetById(request.LikeId, cancellationToken: cancellationToken);
            var likeToAdd = request.UpdatedLikeData.ToLikeForUpdate();
            likeToUpdate.Update(likeToAdd);

            _likeRepository.Update(likeToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}