namespace DriversManagement.Domain.Repairs.Features;

using DriversManagement.Domain.Repairs;
using DriversManagement.Domain.Repairs.Dtos;
using DriversManagement.Domain.Repairs.Services;
using DriversManagement.Services;
using DriversManagement.Domain.Repairs.Models;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateRepair
{
    public sealed record Command(Guid RepairId, RepairForUpdateDto UpdatedRepairData) : IRequest;

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
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateRepair);

            var repairToUpdate = await _repairRepository.GetById(request.RepairId, cancellationToken: cancellationToken);
            var repairToAdd = request.UpdatedRepairData.ToRepairForUpdate();
            repairToUpdate.Update(repairToAdd);

            _repairRepository.Update(repairToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}