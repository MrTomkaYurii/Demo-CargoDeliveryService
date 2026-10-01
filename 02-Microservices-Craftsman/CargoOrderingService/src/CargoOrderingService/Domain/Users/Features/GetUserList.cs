namespace CargoOrderingService.Domain.Users.Features;

using CargoOrderingService.Domain.Users.Dtos;
using CargoOrderingService.Domain.Users.Services;
using CargoOrderingService.Exceptions;
using CargoOrderingService.Resources;
using CargoOrderingService.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QueryKit;
using QueryKit.Configuration;

public static class GetUserList
{
    public sealed record Query(UserParametersDto QueryParameters) : IRequest<PagedList<UserDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IUserRepository userRepository, IHeimGuardClient heimGuard)
        {
            _userRepository = userRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<UserDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadUsers);

            var collection = _userRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToUserDtoQueryable();

            return await PagedList<UserDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}