namespace DriversManagement.Domain.Inspections.Features;

using DriversManagement.Domain.Inspections;
using DriversManagement.Domain.Inspections.Dtos;
using DriversManagement.Domain.Inspections.Services;
using DriversManagement.Services;
using DriversManagement.Domain.Inspections.Models;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateInspection
{
    public sealed record Command(Guid InspectionId, InspectionForUpdateDto UpdatedInspectionData) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
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

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateInspection);

            var inspectionToUpdate = await _inspectionRepository.GetById(request.InspectionId, cancellationToken: cancellationToken);
            var inspectionToAdd = request.UpdatedInspectionData.ToInspectionForUpdate();
            inspectionToUpdate.Update(inspectionToAdd);

            _inspectionRepository.Update(inspectionToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}