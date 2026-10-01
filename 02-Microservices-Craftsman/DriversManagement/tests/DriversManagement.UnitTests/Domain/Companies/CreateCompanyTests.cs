namespace DriversManagement.UnitTests.Domain.Companies;

using DriversManagement.SharedTestHelpers.Fakes.Company;
using DriversManagement.Domain.Companies;
using DriversManagement.Domain.Companies.DomainEvents;
using Bogus;
using FluentAssertions.Extensions;
using ValidationException = DriversManagement.Exceptions.ValidationException;

public class CreateCompanyTests
{
    private readonly Faker _faker;

    public CreateCompanyTests()
    {
        _faker = new Faker();
    }
    
    [Fact]
    public void can_create_valid_company()
    {
        // Arrange
        var companyToCreate = new FakeCompanyForCreation().Generate();
        
        // Act
        var company = Company.Create(companyToCreate);

        // Assert
        company.CompanyName.Should().Be(companyToCreate.CompanyName);
        company.Address.Should().Be(companyToCreate.Address);
        company.Phone.Should().Be(companyToCreate.Phone);
        company.Email.Should().Be(companyToCreate.Email);
        company.ContactEmail.Should().Be(companyToCreate.ContactEmail);
        company.ContactPerson.Should().Be(companyToCreate.ContactPerson);
        company.ContactPhone.Should().Be(companyToCreate.ContactPhone);
    }

    [Fact]
    public void queue_domain_event_on_create()
    {
        // Arrange
        var companyToCreate = new FakeCompanyForCreation().Generate();
        
        // Act
        var company = Company.Create(companyToCreate);

        // Assert
        company.DomainEvents.Count.Should().Be(1);
        company.DomainEvents.FirstOrDefault().Should().BeOfType(typeof(CompanyCreated));
    }
}