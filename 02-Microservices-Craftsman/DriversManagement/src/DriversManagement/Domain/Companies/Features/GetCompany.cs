namespace DriversManagement.Domain.Companies.Features;

using DriversManagement.Domain.Companies.Dtos;
using DriversManagement.Domain.Companies.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;

public static class GetCompany
{
    public sealed record Query(Guid CompanyId) : IRequest<CompanyDto>;

    public sealed class Handler : IRequestHandler<Query, CompanyDto>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ICompanyRepository companyRepository, IHeimGuardClient heimGuard)
        {
            _companyRepository = companyRepository;
            _heimGuard = heimGuard;
        }

        public async Task<CompanyDto> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadCompany);

            var result = await _companyRepository.GetById(request.CompanyId, cancellationToken: cancellationToken);
            return result.ToCompanyDto();
        }
    }
}