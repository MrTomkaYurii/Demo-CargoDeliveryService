namespace DriversManagement.Domain.Inspections.Features;

using DriversManagement.Domain.Inspections.Dtos;
using DriversManagement.Domain.Inspections.Services;
using DriversManagement.Wrappers;
using DriversManagement.Exceptions;
using DriversManagement.Resources;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using Microsoft.EntityFrameworkCore;
using MediatR;
using QueryKit;
using QueryKit.Configuration;

public static class GetAllInspections
{
    public sealed record Query() : IRequest<List<InspectionDto>>;

    public sealed class Handler : IRequestHandler<Query, List<InspectionDto>>
    {
        private readonly IInspectionRepository _inspectionRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IInspectionRepository inspectionRepository, IHeimGuardClient heimGuard)
        {
            _inspectionRepository = inspectionRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<InspectionDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadInspections);

            return _inspectionRepository.Query()
                .AsNoTracking()
                .ToInspectionDtoQueryable()
                .ToList();
        }
    }
}