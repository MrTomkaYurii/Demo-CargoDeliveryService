namespace DriversManagement.SharedTestHelpers.Fakes.Company;

using AutoBogus;
using DriversManagement.Domain.Companies;
using DriversManagement.Domain.Companies.Models;

public sealed class FakeCompanyForUpdate : AutoFaker<CompanyForUpdate>
{
    public FakeCompanyForUpdate()
    {
    }
}