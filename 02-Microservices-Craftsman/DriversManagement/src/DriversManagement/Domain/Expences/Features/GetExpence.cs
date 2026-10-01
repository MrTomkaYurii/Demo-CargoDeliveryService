namespace DriversManagement.Domain.Expences.Features;

using DriversManagement.Domain.Expences.Dtos;
using DriversManagement.Domain.Expences.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetExpence
{
    public sealed record Query(Guid ExpenceId) : IRequest<ExpenceDto>;

    public sealed class Handler : IRequestHandler<Query, ExpenceDto>
    {
        private readonly IExpenceRepository _expenceRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(IExpenceRepository expenceRepository, IHeimGuardClient heimGuard)
        {
            _expenceRepository = expenceRepository;
            _heimGuard = heimGuard;
        }

        public async Task<ExpenceDto> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadExpence);

            var result = await _expenceRepository.GetById(request.ExpenceId, cancellationToken: cancellationToken);
            return result.ToExpenceDto();
        }
    }
}