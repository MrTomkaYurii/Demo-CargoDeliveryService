namespace CargoOrderingService.Domain.RolePermissions.Features;

using CargoOrderingService.Domain.RolePermissions.Services;
using CargoOrderingService.Services;
using CargoOrderingService.Exceptions;
using CargoOrderingService.Domain;
using HeimGuard;
using MediatR;

public static class DeleteRolePermission
{
    public sealed record Command(Guid RolePermissionId) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
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

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeleteRolePermissions);

            var recordToDelete = await _rolePermissionRepository.GetById(request.RolePermissionId, cancellationToken: cancellationToken);
            _rolePermissionRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}