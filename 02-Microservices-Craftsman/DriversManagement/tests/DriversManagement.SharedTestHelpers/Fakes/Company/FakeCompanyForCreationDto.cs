namespace DriversManagement.SharedTestHelpers.Fakes.Company;

using AutoBogus;
using DriversManagement.Domain.Companies;
using DriversManagement.Domain.Companies.Dtos;

public sealed class FakeCompanyForCreationDto : AutoFaker<CompanyForCreationDto>
{
    public FakeCompanyForCreationDto()
    {
    }
}