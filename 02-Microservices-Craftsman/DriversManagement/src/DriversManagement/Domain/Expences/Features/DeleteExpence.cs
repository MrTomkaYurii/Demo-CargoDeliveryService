namespace DriversManagement.Domain.Expences.Features;

using DriversManagement.Domain.Expences.Services;
using DriversManagement.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using MediatR;

public static class DeleteExpence
{
    public sealed record Command(Guid ExpenceId) : IRequest;

    public sealed class Handler : IRequestHandler<Command>
    {
        private readonly IExpenceRepository _expenceRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IExpenceRepository expenceRepository, IUnitOfWork unitOfWork, IHeimGuardClient heimGuard)
        {
            _expenceRepository = expenceRepository;
            _unitOfWork = unitOfWork;
            _heimGuard = heimGuard;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanDeleteExpence);

            var recordToDelete = await _expenceRepository.GetById(request.ExpenceId, cancellationToken: cancellationToken);
            _expenceRepository.Remove(recordToDelete);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}