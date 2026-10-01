namespace DriversManagement.Domain.Inspections.Features;

using DriversManagement.Domain.Inspections.Services;
using DriversManagement.Domain.Inspections;
using DriversManagement.Domain.Inspections.Dtos;
using DriversManagement.Domain.Inspections.Models;
using DriversManagement.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class AddInspection
{
    public sealed record Command(InspectionForCreationDto InspectionToAdd) : IRequest<InspectionDto>;

    public sealed class Handler : IRequestHandler<Command, InspectionDto>
    {
        private readonly IInspectionRepository _inspectionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IInspectionRepository inspectionRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _inspectionRepository = inspectionRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task<InspectionDto> Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanAddInspection);

            var inspectionToAdd = request.InspectionToAdd.ToInspectionForCreation();
            var inspection = Inspection.Create(inspectionToAdd);

            await _inspectionRepository.Add(inspection, cancellationToken);
            await _unitOfWork.CommitChanges(cancellationToken);

            return inspection.ToInspectionDto();
        }
    }
}