namespace DriversManagement.Domain.Users.Features;

using DriversManagement.Domain.Users;
using DriversManagement.Domain.Users.Dtos;
using DriversManagement.Domain.Users.Services;
using DriversManagement.Services;
using DriversManagement.Domain.Users.Models;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateUser
{
    public sealed record Command(Guid UserId, UserForUpdateDto UpdatedUserData) : IRequest;

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
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateUsers);

            var userToUpdate = await _userRepository.GetById(request.UserId, cancellationToken: cancellationToken);
            var userToAdd = request.UpdatedUserData.ToUserForUpdate();
            userToUpdate.Update(userToAdd);

            _userRepository.Update(userToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}