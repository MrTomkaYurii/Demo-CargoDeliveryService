namespace DriversBlogManagement.Domain.BlogUsers.Features;

using DriversBlogManagement.Domain.BlogUsers.Dtos;
using DriversBlogManagement.Domain.BlogUsers.Services;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetBlogUser
{
    public sealed record Query(Guid BlogUserId) : IRequest<BlogUserDto>;

    public sealed class Handler : IRequestHandler<Query, BlogUserDto>
    {
        private readonly IBlogUserRepository _blogUserRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IBlogUserRepository blogUserRepository, IHeimGuardClient heimGuard)
        {
            _blogUserRepository = blogUserRepository;
            _heimGuard = heimGuard;
        }

        public async Task<BlogUserDto> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadUser);

            var result = await _blogUserRepository.GetById(request.BlogUserId, cancellationToken: cancellationToken);
            return result.ToBlogUserDto();
        }
    }
}