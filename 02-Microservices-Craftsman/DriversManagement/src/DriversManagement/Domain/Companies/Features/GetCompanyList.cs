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

public static class GetCompanyList
{
    public sealed record Query(CompanyParametersDto QueryParameters) : IRequest<PagedList<CompanyDto>>;

    public sealed class Handler : IRequestHandler<Query, PagedList<CompanyDto>>
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IHeimGuardClient _heimGuard;

        public Handler(ICompanyRepository companyRepository, IHeimGuardClient heimGuard)
        {
            _companyRepository = companyRepository;
            _heimGuard = heimGuard;
        }

        public async Task<PagedList<CompanyDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadCompanies);

            var collection = _companyRepository.Query().AsNoTracking();

            var queryKitConfig = new CustomQueryKitConfiguration();
            var queryKitData = new QueryKitData()
            {
                Filters = request.QueryParameters.Filters,
                SortOrder = request.QueryParameters.SortOrder,
                Configuration = queryKitConfig
            };
            var appliedCollection = collection.ApplyQueryKit(queryKitData);
            var dtoCollection = appliedCollection.ToCompanyDtoQueryable();

            return await PagedList<CompanyDto>.CreateAsync(dtoCollection,
                request.QueryParameters.PageNumber,
                request.QueryParameters.PageSize,
                cancellationToken);
        }
    }
}