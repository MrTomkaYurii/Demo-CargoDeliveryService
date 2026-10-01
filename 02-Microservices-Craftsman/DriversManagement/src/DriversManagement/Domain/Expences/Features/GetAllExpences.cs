namespace DriversManagement.Domain.Expences.Features;

using DriversManagement.Domain.Expences.Dtos;
using DriversManagement.Domain.Expences.Services;
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

public static class GetAllExpences
{
    public sealed record Query() : IRequest<List<ExpenceDto>>;

    public sealed class Handler : IRequestHandler<Query, List<ExpenceDto>>
    {
        private readonly IExpenceRepository _expenceRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IExpenceRepository expenceRepository, IHeimGuardClient heimGuard)
        {
            _expenceRepository = expenceRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<ExpenceDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadExpences);

            return _expenceRepository.Query()
                .AsNoTracking()
                .ToExpenceDtoQueryable()
                .ToList();
        }
    }
}