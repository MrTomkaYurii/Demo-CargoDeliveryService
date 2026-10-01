namespace DriversManagement.FunctionalTests.FunctionalTests.Companies;

using DriversManagement.SharedTestHelpers.Fakes.Company;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class CreateCompanyTests : TestBase
{
    [Fact]
    public async Task create_company_returns_created_using_valid_dto_and_valid_auth_credentials()
    {
        // Arrange
        var company = new FakeCompanyForCreationDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);

        // Act
        var route = ApiRoutes.Companies.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, company);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }
            
    [Fact]
    public async Task create_company_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var company = new FakeCompanyForCreationDto { }.Generate();

        // Act
        var route = ApiRoutes.Companies.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, company);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task create_company_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var company = new FakeCompanyForCreationDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Companies.Create;
        var result = await FactoryClient.PostJsonRequestAsync(route, company);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}