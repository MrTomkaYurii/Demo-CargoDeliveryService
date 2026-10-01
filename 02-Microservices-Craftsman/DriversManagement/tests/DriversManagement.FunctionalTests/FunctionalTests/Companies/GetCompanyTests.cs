namespace DriversManagement.FunctionalTests.FunctionalTests.Companies;

using DriversManagement.SharedTestHelpers.Fakes.Company;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class GetCompanyTests : TestBase
{
    [Fact]
    public async Task get_company_returns_success_when_entity_exists_using_valid_auth_credentials()
    {
        // Arrange
        var company = new FakeCompanyBuilder().Build();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(company);

        // Act
        var route = ApiRoutes.Companies.GetRecord(company.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
            
    [Fact]
    public async Task get_company_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var company = new FakeCompanyBuilder().Build();

        // Act
        var route = ApiRoutes.Companies.GetRecord(company.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task get_company_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var company = new FakeCompanyBuilder().Build();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Companies.GetRecord(company.Id);
        var result = await FactoryClient.GetRequestAsync(route);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}