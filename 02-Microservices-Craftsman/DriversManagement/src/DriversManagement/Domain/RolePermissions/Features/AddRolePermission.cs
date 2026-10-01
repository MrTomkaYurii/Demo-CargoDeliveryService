namespace DriversManagement.Domain.RolePermissions.Features;

using DriversManagement.Domain.RolePermissions.Services;
using DriversManagement.Domain.RolePermissions;
using DriversManagement.Domain.RolePermissions.Dtos;
using DriversManagement.Domain.RolePermissions.Models;
using DriversManagement.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddRolePermission
{
    public sealed record Command(RolePermissionForCreationDto RolePermissionToAdd) : IRequest<RolePermissionDto>;

    public sealed class Handler : IRequestHandler<Command, RolePermissionDto>
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IRolePermissionRepository rolePermissionRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task<RolePermissionDto> Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddRolePermissions);

            var rolePermissionToAdd = request.RolePermissionToAdd.ToRolePermissionForCreation();
            var rolePermission = RolePermission.Create(rolePermissionToAdd);

            await _rolePermissionRepository.Add(rolePermission, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return rolePermission.ToRolePermissionDto();
        }
    }
}