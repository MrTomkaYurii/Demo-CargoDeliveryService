namespace DriversManagement.IntegrationTests.FeatureTests.Companies;

using DriversManagement.Domain.Companies.Dtos;
using DriversManagement.SharedTestHelpers.Fakes.Company;
using DriversManagement.Domain.Companies.Features;
using Domain;
using System.Threading.Tasks;

public class CompanyListQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_company_list()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var companyOne = new FakeCompanyBuilder().Build();
        var companyTwo = new FakeCompanyBuilder().Build();
        var queryParameters = new CompanyParametersDto();

        await testingServiceScope.InsertAsync(companyOne, companyTwo);

        // Act
        var query = new GetCompanyList.Query(queryParameters);
        var companies = await testingServiceScope.SendAsync(query);

        // Assert
        companies.Count.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadCompanies);
        var queryParameters = new CompanyParametersDto();

        // Act
        var command = new GetCompanyList.Query(queryParameters);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}