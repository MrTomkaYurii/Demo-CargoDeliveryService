namespace DriversBlogManagement.Domain.BlogUsers.Features;

using DriversBlogManagement.Domain.BlogUsers.Services;
using DriversBlogManagement.Domain.BlogUsers;
using DriversBlogManagement.Domain.BlogUsers.Dtos;
using DriversBlogManagement.Domain.BlogUsers.Models;
using DriversBlogManagement.Services;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddBlogUser
{
    public sealed record Command(BlogUserForCreationDto BlogUserToAdd) : IRequest<BlogUserDto>;

    public sealed class Handler : IRequestHandler<Command, BlogUserDto>
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

        public async Task<BlogUserDto> Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddUser);

            var blogUserToAdd = request.BlogUserToAdd.ToBlogUserForCreation();
            var blogUser = BlogUser.Create(blogUserToAdd);

            await _blogUserRepository.Add(blogUser, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return blogUser.ToBlogUserDto();
        }
    }
}