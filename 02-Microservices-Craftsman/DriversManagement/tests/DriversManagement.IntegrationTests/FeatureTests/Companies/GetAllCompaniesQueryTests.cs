namespace DriversManagement.IntegrationTests.FeatureTests.Companies;

using DriversManagement.Domain.Companies.Dtos;
using DriversManagement.SharedTestHelpers.Fakes.Company;
using DriversManagement.Domain.Companies.Features;
using Domain;
using System.Threading.Tasks;

public class GetAllCompaniesQueryTests : TestBase
{
    
    [Fact]
    public async Task can_get_all_companies()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var companyOne = new FakeCompanyBuilder().Build();
        var companyTwo = new FakeCompanyBuilder().Build();

        await testingServiceScope.InsertAsync(companyOne, companyTwo);

        // Act
        var query = new GetAllCompanies.Query();
        var companies = await testingServiceScope.SendAsync(query);

        // Assert
        companies.Count.Should().BeGreaterThanOrEqualTo(2);
        companies.FirstOrDefault(x => x.Id == companyOne.Id).Should().NotBeNull();
        companies.FirstOrDefault(x => x.Id == companyTwo.Id).Should().NotBeNull();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadCompanies);

        // Act
        var query = new GetAllCompanies.Query();
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}