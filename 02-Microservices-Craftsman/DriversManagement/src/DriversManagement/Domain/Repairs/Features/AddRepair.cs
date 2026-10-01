namespace DriversManagement.Domain.Repairs.Features;

using DriversManagement.Domain.Repairs.Services;
using DriversManagement.Domain.Repairs;
using DriversManagement.Domain.Repairs.Dtos;
using DriversManagement.Domain.Repairs.Models;
using DriversManagement.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddRepair
{
    public sealed record Command(RepairForCreationDto RepairToAdd) : IRequest<RepairDto>;

    public sealed class Handler : IRequestHandler<Command, RepairDto>
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

        public async Task<RepairDto> Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddRepair);

            var repairToAdd = request.RepairToAdd.ToRepairForCreation();
            var repair = Repair.Create(repairToAdd);

            await _repairRepository.Add(repair, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return repair.ToRepairDto();
        }
    }
}