namespace DriversManagement.IntegrationTests.FeatureTests.Companies;

using DriversManagement.SharedTestHelpers.Fakes.Company;
using DriversManagement.Domain.Companies.Features;
using Domain;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class CompanyQueryTests : TestBase
{
    [Fact]
    public async Task can_get_existing_company_with_accurate_props()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var companyOne = new FakeCompanyBuilder().Build();
        await testingServiceScope.InsertAsync(companyOne);

        // Act
        var query = new GetCompany.Query(companyOne.Id);
        var company = await testingServiceScope.SendAsync(query);

        // Assert
        company.CompanyName.Should().Be(companyOne.CompanyName);
        company.Address.Should().Be(companyOne.Address);
        company.Phone.Should().Be(companyOne.Phone);
        company.Email.Should().Be(companyOne.Email);
        company.ContactEmail.Should().Be(companyOne.ContactEmail);
        company.ContactPerson.Should().Be(companyOne.ContactPerson);
        company.ContactPhone.Should().Be(companyOne.ContactPhone);
    }

    [Fact]
    public async Task get_company_throws_notfound_exception_when_record_does_not_exist()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        var badId = Guid.NewGuid();

        // Act
        var query = new GetCompany.Query(badId);
        Func<Task> act = () => testingServiceScope.SendAsync(query);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task must_be_permitted()
    {
        // Arrange
        var testingServiceScope = new TestingServiceScope();
        testingServiceScope.SetUserNotPermitted(Permissions.CanReadCompany);

        // Act
        var command = new GetCompany.Query(Guid.NewGuid());
        Func<Task> act = () => testingServiceScope.SendAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
    }
}