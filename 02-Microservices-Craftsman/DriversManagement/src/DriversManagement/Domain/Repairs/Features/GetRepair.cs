namespace DriversManagement.Domain.Repairs.Features;

using DriversManagement.Domain.Repairs.Dtos;
using DriversManagement.Domain.Repairs.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetRepair
{
    public sealed record Query(Guid RepairId) : IRequest<RepairDto>;

    public sealed class Handler : IRequestHandler<Query, RepairDto>
    {
        private readonly IRepairRepository _repairRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IRepairRepository repairRepository, IHeimGuardClient heimGuard)
        {
            _repairRepository = repairRepository;
            _heimGuard = heimGuard;
        }

        public async Task<RepairDto> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadRepair);

            var result = await _repairRepository.GetById(request.RepairId, cancellationToken: cancellationToken);
            return result.ToRepairDto();
        }
    }
}