namespace CargoOrderingService.Domain.Users.Features;

using CargoOrderingService.Domain.Users.Services;
using CargoOrderingService.Domain.Users;
using CargoOrderingService.Domain.Users.Dtos;
using CargoOrderingService.Services;
using CargoOrderingService.Exceptions;
using HeimGuard;
using Mappings;
using MediatR;
using Roles;

public static class AddUserRole
{
    public sealed record Command(Guid UserId, string Role, bool SkipPermissions = false) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IUserRepository userRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            if(!request.SkipPermissions)
                await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddUserRoles);
            
            var user = await _userRepository.GetById(request.UserId, true, cancellationToken);

            var roleToAdd = user.AddRole(new Role(request.Role));
            await _userRepository.AddRole(roleToAdd, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}