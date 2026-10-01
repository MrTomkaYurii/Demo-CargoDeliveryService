namespace DriversManagement.IntegrationTests.FeatureTests.Companies;

using DriversManagement.SharedTestHelpers.Fakes.Company;
using DriversManagement.Domain.Companies.Dtos;
using DriversManagement.Domain.Companies.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class UpdateCompanyCommandTests : TestBase
{
    [Fact]
    public async Task can_update_existing_company_in_db()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var company = new FakeCompanyBuilder().Build();
        await testingServiceScope.InsertAsync(company);
        var updatedCompanyDto = new FakeCompanyForUpdateDto().Generate();

        // Act
        var command = new UpdateCompany.Command(company.Id, updatedCompanyDto);
        await testingServiceScope.SendAsync(command);
        var updatedCompany = await testingServiceScope
            .ExecuteDbContextAsync(db => db.Companies
                .FirstOrDefaultAsync(c => c.Id == company.Id));

        // Assert
        updatedCompany.CompanyName.Should().Be(updatedCompanyDto.CompanyName);
        updatedCompany.Address.Should().Be(updatedCompanyDto.Address);
        updatedCompany.Phone.Should().Be(updatedCompanyDto.Phone);
        updatedCompany.Email.Should().Be(updatedCompanyDto.Email);
        updatedCompany.ContactEmail.Should().Be(updatedCompanyDto.ContactEmail);
        updatedCompany.ContactPerson.Should().Be(updatedCompanyDto.ContactPerson);
        updatedCompany.ContactPhone.Should().Be(updatedCompanyDto.ContactPhone);
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanUpdateCompany);
        var companyOne = new FakeCompanyForUpdateDto();

        // Act
        var command = new UpdateCompany.Command(Guid.NewGuid(), companyOne);
        var act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}