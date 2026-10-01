namespace DriversManagement.Domain.Trucks.Features;

using DriversManagement.Domain.Trucks.Services;
using DriversManagement.Domain.Trucks;
using DriversManagement.Domain.Trucks.Dtos;
using DriversManagement.Domain.Trucks.Models;
using DriversManagement.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddTruck
{
    public sealed record Command(TruckForCreationDto TruckToAdd) : IRequest<TruckDto>;

    public sealed class Handler : IRequestHandler<Command, TruckDto>
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

        public async Task<TruckDto> Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddTruck);

            var truckToAdd = request.TruckToAdd.ToTruckForCreation();
            var truck = Truck.Create(truckToAdd);

            await _truckRepository.Add(truck, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return truck.ToTruckDto();
        }
    }
}