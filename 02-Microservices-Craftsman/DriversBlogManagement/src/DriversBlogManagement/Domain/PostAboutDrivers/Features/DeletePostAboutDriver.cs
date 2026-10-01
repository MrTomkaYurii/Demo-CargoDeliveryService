namespace DriversBlogManagement.Domain.PostAboutDrivers.Features;

using DriversBlogManagement.Domain.PostAboutDrivers.Services;
using DriversBlogManagement.Services;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Domain;
using HeimGuard;
using MediatR;

public static class DeletePostAboutDriver
{
    public sealed record Command(Guid PostAboutDriverId) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IPostAboutDriverRepository _postAboutDriverRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IPostAboutDriverRepository postAboutDriverRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _postAboutDriverRepository = postAboutDriverRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeletePostAboutDriver);

            var recordToDelete = await _postAboutDriverRepository.GetById(request.PostAboutDriverId, cancellationToken: cancellationToken);
            _postAboutDriverRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}