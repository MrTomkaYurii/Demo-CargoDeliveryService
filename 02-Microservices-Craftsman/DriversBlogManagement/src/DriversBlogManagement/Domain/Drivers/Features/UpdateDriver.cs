namespace DriversBlogManagement.Domain.Drivers.Features;

using DriversBlogManagement.Domain.Drivers;
using DriversBlogManagement.Domain.Drivers.Dtos;
using DriversBlogManagement.Domain.Drivers.Services;
using DriversBlogManagement.Services;
using DriversBlogManagement.Domain.Drivers.Models;
using DriversBlogManagement.Exceptions;
using DriversBlogManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateDriver
{
    public sealed record Command(Guid DriverId, DriverForUpdateDto UpdatedDriverData) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IDriverRepository driverRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _driverRepository = driverRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateDriver);

            var driverToUpdate = await _driverRepository.GetById(request.DriverId, cancellationToken: cancellationToken);
            var driverToAdd = request.UpdatedDriverData.ToDriverForUpdate();
            driverToUpdate.Update(driverToAdd);

            _driverRepository.Update(driverToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}