namespace DriversManagement.SharedTestHelpers.Fakes.Company;

using AutoBogus;
using DriversManagement.Domain.Companies;
using DriversManagement.Domain.Companies.Dtos;

public sealed class FakeCompanyForUpdateDto : AutoFaker<CompanyForUpdateDto>
{
    public FakeCompanyForUpdateDto()
    {
    }
}