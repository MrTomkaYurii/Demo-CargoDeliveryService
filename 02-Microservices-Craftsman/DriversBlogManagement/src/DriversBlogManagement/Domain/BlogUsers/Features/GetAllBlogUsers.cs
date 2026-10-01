namespace DriversBlogManagement.Domain.BlogUsers.Features;

using DriversBlogManagement.Domain.BlogUsers.Dtos;
using DriversBlogManagement.Domain.BlogUsers.Services;
using DriversBlogManagement.Wrappers;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Resources;
using DriversBlogManagement.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QueryKit;
using QueryKit.Configuration;

public static class GetAllBlogUsers
{
    public sealed record Query() : IRequest<List<BlogUserDto>>;

    public sealed class Handler : IRequestHandler<Query, List<BlogUserDto>>
    {
        private readonly IBlogUserRepository _blogUserRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IBlogUserRepository blogUserRepository, IHeimGuardClient heimGuard)
        {
            _blogUserRepository = blogUserRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<BlogUserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadUsers);

            return _blogUserRepository.Query()
                .AsNoTracking()
                .ToBlogUserDtoQueryable()
                .ToList();
        }
    }
}