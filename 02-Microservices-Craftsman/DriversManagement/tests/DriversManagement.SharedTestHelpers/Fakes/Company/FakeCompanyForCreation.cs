namespace DriversManagement.SharedTestHelpers.Fakes.Company;

using AutoBogus;
using DriversManagement.Domain.Companies;
using DriversManagement.Domain.Companies.Models;

public sealed class FakeCompanyForCreation : AutoFaker<CompanyForCreation>
{
    public FakeCompanyForCreation()
    {
    }
}