namespace DriversManagement.SharedTestHelpers.Fakes.Company;

using DriversManagement.Domain.Companies;
using DriversManagement.Domain.Companies.Models;

public class FakeCompanyBuilder
{
    private CompanyForCreation _creationData = new FakeCompanyForCreation().Generate();

    public FakeCompanyBuilder WithModel(CompanyForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakeCompanyBuilder WithCompanyName(string companyName)
    {
        _creationData.CompanyName = companyName;
        return this;
    }
    
    public FakeCompanyBuilder WithAddress(string address)
    {
        _creationData.Address = address;
        return this;
    }
    
    public FakeCompanyBuilder WithPhone(string phone)
    {
        _creationData.Phone = phone;
        return this;
    }
    
    public FakeCompanyBuilder WithEmail(string email)
    {
        _creationData.Email = email;
        return this;
    }
    
    public FakeCompanyBuilder WithContactEmail(string contactEmail)
    {
        _creationData.ContactEmail = contactEmail;
        return this;
    }
    
    public FakeCompanyBuilder WithContactPerson(string contactPerson)
    {
        _creationData.ContactPerson = contactPerson;
        return this;
    }
    
    public FakeCompanyBuilder WithContactPhone(string contactPhone)
    {
        _creationData.ContactPhone = contactPhone;
        return this;
    }
    
    public Company Build()
    {
        var result = Company.Create(_creationData);
        return result;
    }
}