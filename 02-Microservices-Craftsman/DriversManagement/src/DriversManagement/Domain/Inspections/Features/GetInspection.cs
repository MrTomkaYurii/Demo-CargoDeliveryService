namespace DriversManagement.Domain.Inspections.Features;

using DriversManagement.Domain.Inspections.Dtos;
using DriversManagement.Domain.Inspections.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetInspection
{
    public sealed record Query(Guid InspectionId) : IRequest<InspectionDto>;

    public sealed class Handler : IRequestHandler<Query, InspectionDto>
    {
        private readonly IInspectionRepository _inspectionRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IInspectionRepository inspectionRepository, IHeimGuardClient heimGuard)
        {
            _inspectionRepository = inspectionRepository;
            _heimGuard = heimGuard;
        }

        public async Task<InspectionDto> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadInspection);

            var result = await _inspectionRepository.GetById(request.InspectionId, cancellationToken: cancellationToken);
            return result.ToInspectionDto();
        }
    }
}