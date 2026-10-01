namespace DriversManagement.Domain.Inspections.Features;

using DriversManagement.Domain.Inspections.Services;
using DriversManagement.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using MediatR;

public static class DeleteInspection
{
    public sealed record Command(Guid InspectionId) : IRequest;

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
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeleteInspection);

            var recordToDelete = await _inspectionRepository.GetById(request.InspectionId, cancellationToken: cancellationToken);
            _inspectionRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}