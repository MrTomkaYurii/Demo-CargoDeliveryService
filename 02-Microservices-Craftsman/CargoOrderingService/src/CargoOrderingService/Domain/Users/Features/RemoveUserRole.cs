namespace CargoOrderingService.Domain.Users.Features;

using CargoOrderingService.Domain.Users.Services;
using CargoOrderingService.Domain.Users;
using CargoOrderingService.Domain.Users.Dtos;
using CargoOrderingService.Services;
using CargoOrderingService.Exceptions;
using HeimGuard;
using MediatR;
using Roles;

public static class RemoveUserRole
{
    public sealed record Command(Guid UserId, string Role) : IRequest;

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
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanRemoveUserRoles);
            var user = await _userRepository.GetById(request.UserId, true, cancellationToken);

            var roleToRemove = user.RemoveRole(new Role(request.Role));
            _userRepository.RemoveRole(roleToRemove);
            _userRepository.Update(user);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}