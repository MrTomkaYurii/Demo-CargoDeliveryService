namespace DriversBlogManagement.Domain.BlogUsers.Features;

using DriversBlogManagement.Domain.BlogUsers;
using DriversBlogManagement.Domain.BlogUsers.Dtos;
using DriversBlogManagement.Domain.BlogUsers.Services;
using DriversBlogManagement.Services;
using DriversBlogManagement.Domain.BlogUsers.Models;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateBlogUser
{
    public sealed record Command(Guid BlogUserId, BlogUserForUpdateDto UpdatedBlogUserData) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IBlogUserRepository _blogUserRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IBlogUserRepository blogUserRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _blogUserRepository = blogUserRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateUser);

            var blogUserToUpdate = await _blogUserRepository.GetById(request.BlogUserId, cancellationToken: cancellationToken);
            var blogUserToAdd = request.UpdatedBlogUserData.ToBlogUserForUpdate();
            blogUserToUpdate.Update(blogUserToAdd);

            _blogUserRepository.Update(blogUserToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}