namespace DriversBlogManagement.Domain.PostAboutDrivers.Features;

using DriversBlogManagement.Domain.PostAboutDrivers;
using DriversBlogManagement.Domain.PostAboutDrivers.Dtos;
using DriversBlogManagement.Domain.PostAboutDrivers.Services;
using DriversBlogManagement.Services;
using DriversBlogManagement.Domain.PostAboutDrivers.Models;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdatePostAboutDriver
{
    public sealed record Command(Guid PostAboutDriverId, PostAboutDriverForUpdateDto UpdatedPostAboutDriverData) : IRequest;

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
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdatePostAboutDriver);

            var postAboutDriverToUpdate = await _postAboutDriverRepository.GetById(request.PostAboutDriverId, cancellationToken: cancellationToken);
            var postAboutDriverToAdd = request.UpdatedPostAboutDriverData.ToPostAboutDriverForUpdate();
            postAboutDriverToUpdate.Update(postAboutDriverToAdd);

            _postAboutDriverRepository.Update(postAboutDriverToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}