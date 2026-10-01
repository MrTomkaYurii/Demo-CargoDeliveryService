namespace DriversBlogManagement.Domain.Comments.Features;

using DriversBlogManagement.Domain.Comments.Services;
using DriversBlogManagement.Services;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Domain;
using HeimGuard;
using MediatR;

public static class DeleteComment
{
    public sealed record Command(Guid CommentId) : IRequest;

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
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeleteComment);

            var recordToDelete = await _commentRepository.GetById(request.CommentId, cancellationToken: cancellationToken);
            _commentRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}