namespace DriversManagement.Domain.Expences.Features;

using DriversManagement.Domain.Expences;
using DriversManagement.Domain.Expences.Dtos;
using DriversManagement.Domain.Expences.Services;
using DriversManagement.Services;
using DriversManagement.Domain.Expences.Models;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class UpdateExpence
{
    public sealed record Command(Guid ExpenceId, ExpenceForUpdateDto UpdatedExpenceData) : IRequest;

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
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanUpdateExpence);

            var expenceToUpdate = await _expenceRepository.GetById(request.ExpenceId, cancellationToken: cancellationToken);
            var expenceToAdd = request.UpdatedExpenceData.ToExpenceForUpdate();
            expenceToUpdate.Update(expenceToAdd);

            _expenceRepository.Update(expenceToUpdate);
            await _unitOfWork.CommitChanges(cancellationToken);
        }
    }
}