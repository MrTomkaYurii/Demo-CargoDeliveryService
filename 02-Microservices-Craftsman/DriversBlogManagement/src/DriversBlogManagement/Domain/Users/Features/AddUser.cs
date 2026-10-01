namespace DriversBlogManagement.Domain.Users.Features;

using DriversBlogManagement.Domain.Users.Services;
using DriversBlogManagement.Domain.Users;
using DriversBlogManagement.Domain.Users.Dtos;
using DriversBlogManagement.Domain.Users.Models;
using DriversBlogManagement.Services;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddUser
{
    public sealed record Command(UserForCreationDto UserToAdd, bool SkipPermissions = false) : IRequest<UserDto>;

    public sealed class Handler : IRequestHandler<Command, UserDto>
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

        public async Task<UserDto> Handle(Command request, CancellationToken cancellationToken)
        {
            if(!request.SkipPermissions)
                await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddUsers);

            var userToAdd = request.UserToAdd.ToUserForCreation();
            var user = User.Create(userToAdd);
            await _userRepository.Add(user, cancellationToken);

            await _unitOfWork.CommitChanges(cancellationToken);

            var userAdded = await _userRepository.GetById(user.Id, cancellationToken: cancellationToken);
            return userAdded.ToUserDto();
        }
    }
}
