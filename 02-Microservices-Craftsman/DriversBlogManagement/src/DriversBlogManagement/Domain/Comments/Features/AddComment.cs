namespace DriversBlogManagement.Domain.Comments.Features;

using DriversBlogManagement.Domain.Comments.Services;
using DriversBlogManagement.Domain.Comments;
using DriversBlogManagement.Domain.Comments.Dtos;
using DriversBlogManagement.Domain.Comments.Models;
using DriversBlogManagement.Services;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddComment
{
    public sealed record Command(CommentForCreationDto CommentToAdd) : IRequest<CommentDto>;

    public sealed class Handler : IRequestHandler<Command, CommentDto>
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

        public async Task<CommentDto> Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddComment);

            var commentToAdd = request.CommentToAdd.ToCommentForCreation();
            var comment = Comment.Create(commentToAdd);

            await _commentRepository.Add(comment, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return comment.ToCommentDto();
        }
    }
}