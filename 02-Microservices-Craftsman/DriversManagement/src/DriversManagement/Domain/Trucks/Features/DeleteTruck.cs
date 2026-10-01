namespace DriversManagement.Domain.Trucks.Features;

using DriversManagement.Domain.Trucks.Services;
using DriversManagement.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using MediatR;

public static class DeleteTruck
{
    public sealed record Command(Guid TruckId) : IRequest;

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
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeleteTruck);

            var recordToDelete = await _truckRepository.GetById(request.TruckId, cancellationToken: cancellationToken);
            _truckRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}