namespace DriversManagement.Domain.Trucks.Features;

using DriversManagement.Domain.Trucks;
using DriversManagement.Domain.Trucks.Dtos;
using DriversManagement.Domain.Trucks.Services;
using DriversManagement.Services;
using DriversManagement.Domain.Trucks.Models;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateTruck
{
    public sealed record Command(Guid TruckId, TruckForUpdateDto UpdatedTruckData) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly ITruckRepository _truckRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ITruckRepository truckRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _truckRepository = truckRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateTruck);

            var truckToUpdate = await _truckRepository.GetById(request.TruckId, cancellationToken: cancellationToken);
            var truckToAdd = request.UpdatedTruckData.ToTruckForUpdate();
            truckToUpdate.Update(truckToAdd);

            _truckRepository.Update(truckToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}