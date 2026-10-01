namespace DriversManagement.Domain.Companies.Features;

using DriversManagement.Domain.Companies.Dtos;
using DriversManagement.Domain.Companies.Services;
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

public static class GetAllCompanies
{
    public sealed record Query() : IRequest<List<CompanyDto>>;

    public sealed class Handler : IRequestHandler<Query, List<CompanyDto>>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ICompanyRepository companyRepository, IHeimGuardClient heimGuard)
        {
            _companyRepository = companyRepository;
            _heimGuard = heimGuard;
        }

        public async Task<List<CompanyDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadCompanies);

            return _companyRepository.Query()
                .AsNoTracking()
                .ToCompanyDtoQueryable()
                .ToList();
        }
    }
}