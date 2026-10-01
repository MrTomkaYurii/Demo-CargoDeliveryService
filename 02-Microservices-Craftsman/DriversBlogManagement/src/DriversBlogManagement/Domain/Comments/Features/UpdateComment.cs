namespace DriversBlogManagement.Domain.Comments.Features;

using DriversBlogManagement.Domain.Comments;
using DriversBlogManagement.Domain.Comments.Dtos;
using DriversBlogManagement.Domain.Comments.Services;
using DriversBlogManagement.Services;
using DriversBlogManagement.Domain.Comments.Models;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateComment
{
    public sealed record Command(Guid CommentId, CommentForUpdateDto UpdatedCommentData) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ICommentRepository commentRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _commentRepository = commentRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateComment);

            var commentToUpdate = await _commentRepository.GetById(request.CommentId, cancellationToken: cancellationToken);
            var commentToAdd = request.UpdatedCommentData.ToCommentForUpdate();
            commentToUpdate.Update(commentToAdd);

            _commentRepository.Update(commentToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}