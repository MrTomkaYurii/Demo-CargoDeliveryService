namespace DriversManagement.Domain.Repairs.Features;

using DriversManagement.Domain.Repairs.Services;
using DriversManagement.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using MediatR;

public static class DeleteRepair
{
    public sealed record Command(Guid RepairId) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IRepairRepository _repairRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IRepairRepository repairRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _repairRepository = repairRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeleteRepair);

            var recordToDelete = await _repairRepository.GetById(request.RepairId, cancellationToken: cancellationToken);
            _repairRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}