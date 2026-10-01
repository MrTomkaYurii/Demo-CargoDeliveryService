namespace DriversManagement.FunctionalTests.FunctionalTests.Companies;

using DriversManagement.SharedTestHelpers.Fakes.Company;
using DriversManagement.FunctionalTests.TestUtilities;
using DriversManagement.Domain;
using System.Net;
using System.Threading.Tasks;

public class UpdateCompanyRecordTests : TestBase
{
    [Fact]
    public async Task put_company_returns_nocontent_when_entity_exists_and_auth_credentials_are_valid()
    {
        // Arrange
        var company = new FakeCompanyBuilder().Build();
        var updatedCompanyDto = new FakeCompanyForUpdateDto().Generate();

        var callingUser = await AddNewSuperAdmin();
        FactoryClient.AddAuth(callingUser.Identifier);
        await InsertAsync(company);

        // Act
        var route = ApiRoutes.Companies.Put(company.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedCompanyDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
            
    [Fact]
    public async Task put_company_returns_unauthorized_without_valid_token()
    {
        // Arrange
        var company = new FakeCompanyBuilder().Build();
        var updatedCompanyDto = new FakeCompanyForUpdateDto { }.Generate();

        // Act
        var route = ApiRoutes.Companies.Put(company.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedCompanyDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
            
    [Fact]
    public async Task put_company_returns_forbidden_without_proper_scope()
    {
        // Arrange
        var company = new FakeCompanyBuilder().Build();
        var updatedCompanyDto = new FakeCompanyForUpdateDto { }.Generate();
        FactoryClient.AddAuth();

        // Act
        var route = ApiRoutes.Companies.Put(company.Id);
        var result = await FactoryClient.PutJsonRequestAsync(route, updatedCompanyDto);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}