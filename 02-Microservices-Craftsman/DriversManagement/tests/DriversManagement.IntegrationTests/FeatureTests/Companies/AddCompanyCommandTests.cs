namespace DriversManagement.IntegrationTests.FeatureTests.Companies;

using DriversManagement.SharedTestHelpers.Fakes.Company;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DriversManagement.Domain.Companies.Features;

public class AddCompanyCommandTests : TestBase
{
    [Fact]
    public async Task can_add_new_company_to_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var companyOne = new FakeCompanyForCreationDto().Generate();

        // Act
        var command = new AddCompany.Command(companyOne);
        var companyReturned = await testingServiceScope.SendAsync(command);
        var companyCreated = await testingServiceScope.ExecuteDbContextAsync(db => db.Companies
            .FirstOrDefaultAsync(c => c.Id == companyReturned.Id));

        // Assert
        companyReturned.CompanyName.Should().Be(companyOne.CompanyName);
        companyReturned.Address.Should().Be(companyOne.Address);
        companyReturned.Phone.Should().Be(companyOne.Phone);
        companyReturned.Email.Should().Be(companyOne.Email);
        companyReturned.ContactEmail.Should().Be(companyOne.ContactEmail);
        companyReturned.ContactPerson.Should().Be(companyOne.ContactPerson);
        companyReturned.ContactPhone.Should().Be(companyOne.ContactPhone);

        companyCreated.CompanyName.Should().Be(companyOne.CompanyName);
        companyCreated.Address.Should().Be(companyOne.Address);
        companyCreated.Phone.Should().Be(companyOne.Phone);
        companyCreated.Email.Should().Be(companyOne.Email);
        companyCreated.ContactEmail.Should().Be(companyOne.ContactEmail);
        companyCreated.ContactPerson.Should().Be(companyOne.ContactPerson);
        companyCreated.ContactPhone.Should().Be(companyOne.ContactPhone);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanAddCompany);
        var companyOne = new FakeCompanyForCreationDto();

        // Act
        var command = new AddCompany.Command(companyOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}