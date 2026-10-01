namespace CargoOrderingService.Domain.RolePermissions.Features;

using CargoOrderingService.Domain.RolePermissions.Dtos;
using CargoOrderingService.Domain.RolePermissions.Services;
using CargoOrderingService.Exceptions;
using CargoOrderingService.Resources;
using CargoOrderingService.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QueryKit;
using QueryKit.Configuration;

public static class GetRolePermissionList
{
    public sealed record Query(RolePermissionParametersDto QueryParameters) : IRequest<PagedList<RolePermissionDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<RolePermissionDto>>
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IRolePermissionRepository rolePermissionRepository, IHeimGuardClient heimGuard)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<RolePermissionDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadRolePermissions);

            var collection = _rolePermissionRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToRolePermissionDtoQueryable();

            return await PagedList<RolePermissionDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}