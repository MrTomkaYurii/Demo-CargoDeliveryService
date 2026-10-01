namespace DriversManagement.Domain.RolePermissions.Features;

using DriversManagement.Domain.RolePermissions.Dtos;
using DriversManagement.Domain.RolePermissions.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetRolePermission
{
    public sealed record Query(Guid RolePermissionId) : IRequest<RolePermissionDto>;

    public sealed class Handler : IRequestHandler<Query, RolePermissionDto>
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IRolePermissionRepository rolePermissionRepository, IHeimGuardClient heimGuard)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _heimGuard = heimGuard;
        }

        public async Task<RolePermissionDto> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadRolePermissions);

            var result = await _rolePermissionRepository.GetById(request.RolePermissionId, cancellationToken: cancellationToken);
            return result.ToRolePermissionDto();
        }
    }
}