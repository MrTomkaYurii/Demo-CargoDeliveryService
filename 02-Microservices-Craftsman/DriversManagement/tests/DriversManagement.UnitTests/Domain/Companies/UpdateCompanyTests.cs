namespace DriversManagement.UnitTests.Domain.Companies;

using DriversManagement.SharedTestHelpers.Fakes.Company;
using DriversManagement.Domain.Companies;
using DriversManagement.Domain.Companies.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class UpdateCompanyTests
{
    private readonly Faker _faker;

    public UpdateCompanyTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_update_company()
    {
        // Arrange
        var company = new FakeCompanyBuilder().Build();
        var updatedCompany = new FakeCompanyForUpdate().Generate();
        
        // Act
        company.Update(updatedCompany);

        // Assert
        company.CompanyName.Should().Be(updatedCompany.CompanyName);
        company.Address.Should().Be(updatedCompany.Address);
        company.Phone.Should().Be(updatedCompany.Phone);
        company.Email.Should().Be(updatedCompany.Email);
        company.ContactEmail.Should().Be(updatedCompany.ContactEmail);
        company.ContactPerson.Should().Be(updatedCompany.ContactPerson);
        company.ContactPhone.Should().Be(updatedCompany.ContactPhone);
    }
    
    [Fact]
    public void queue_domain_event_on_update()
    {
        // Arrange
        var company = new FakeCompanyBuilder().Build();
        var updatedCompany = new FakeCompanyForUpdate().Generate();
        company.DomainEvents.Clear();
        
        // Act
        company.Update(updatedCompany);

        // Assert
        company.DomainEvents.Count.Should().Be(1);
        company.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(CompanyUpdated));
    }
}