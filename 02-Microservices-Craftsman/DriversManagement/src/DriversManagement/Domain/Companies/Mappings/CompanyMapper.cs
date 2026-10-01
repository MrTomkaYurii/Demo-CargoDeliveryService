namespace DriversManagement.Domain.Companies.Mappings;

using DriversManagement.Domain.Companies.Dtos;
using DriversManagement.Domain.Companies.Models;
using Riok.Mapperly.Abstractions;

[Mapper]
public static partial class CompanyMapper
{
    public static partial CompanyForCreation ToCompanyForCreation(this CompanyForCreationDto companyForCreationDto);
    public static partial CompanyForUpdate ToCompanyForUpdate(this CompanyForUpdateDto companyForUpdateDto);
    public static partial CompanyDto ToCompanyDto(this Company company);
    public static partial IQueryable<CompanyDto> ToCompanyDtoQueryable(this IQueryable<Company> company);
}