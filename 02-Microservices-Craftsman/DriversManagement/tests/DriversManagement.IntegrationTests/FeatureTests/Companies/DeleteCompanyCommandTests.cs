namespace DriversManagement.IntegrationTests.FeatureTests.Companies;

using DriversManagement.SharedTestHelpers.Fakes.Company;
using DriversManagement.Domain.Companies.Features;
using Microsoft.EntityFrameworkCore;
using Domain;
using System.Threading.Tasks;

public class DeleteCompanyCommandTests : TestBase
{
    [Fact]
    public async Task can_delete_company_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var company = new FakeCompanyBuilder().Build();
        await testingServiceScope.InsertAsync(company);

        // Act
        var command = new DeleteCompany.Command(company.Id);
        await testingServiceScope.SendAsync(command);
        var companyResponse = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Companies
                .CountAsync(c => c.Id == company.Id));

        // Assert
        companyResponse.Should().Be(0);
    }

    [Fact]
    public async Task delete_company_throws_notfoundexception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var command = new DeleteCompany.Command(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task can_softdelete_company_from_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var company = new FakeCompanyBuilder().Build();
        await testingServiceScope.InsertAsync(company);

        // Act
        var command = new DeleteCompany.Command(company.Id);
        await testingServiceScope.SendAsync(command);
        var deletedCompany = await testingServiceScope.ExecuteDbContextAsync(db => db.Companies
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == company.Id));

        // Assert
        deletedCompany?.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanDeleteCompany);

        // Act
        var command = new DeleteCompany.Command(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}