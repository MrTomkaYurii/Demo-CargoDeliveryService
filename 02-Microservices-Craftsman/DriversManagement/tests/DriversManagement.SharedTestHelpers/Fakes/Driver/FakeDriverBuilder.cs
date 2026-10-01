namespace DriversManagement.SharedTestHelpers.Fakes.Driver;

using DriversManagement.Domain.Drivers;
using DriversManagement.Domain.Drivers.Models;

public class FakeDriverBuilder
{
    private DriverForCreation _creationData = new FakeDriverForCreation().Generate();

    public FakeDriverBuilder WithModel(DriverForCreation model)
    {
        _creationData = model;
        return this;
    }
    
    public FakeDriverBuilder WithFirstName(string firstName)
    {
        _creationData.FirstName = firstName;
        return this;
    }
    
    public FakeDriverBuilder WithLastName(string lastName)
    {
        _creationData.LastName = lastName;
        return this;
    }
    
    public FakeDriverBuilder WithMiddleName(string middleName)
    {
        _creationData.MiddleName = middleName;
        return this;
    }
    
    public FakeDriverBuilder WithGender(string gender)
    {
        _creationData.Gender = gender;
        return this;
    }
    
    public FakeDriverBuilder WithBirthdate(string birthdate)
    {
        _creationData.Birthdate = birthdate;
        return this;
    }
    
    public FakeDriverBuilder WithPlaceOfBirth(string placeOfBirth)
    {
        _creationData.PlaceOfBirth = placeOfBirth;
        return this;
    }
    
    public FakeDriverBuilder WithNationality(string nationality)
    {
        _creationData.Nationality = nationality;
        return this;
    }
    
    public FakeDriverBuilder WithMaritalStatus(string maritalStatus)
    {
        _creationData.MaritalStatus = maritalStatus;
        return this;
    }
    
    public FakeDriverBuilder WithIdentificationType(string identificationType)
    {
        _creationData.IdentificationType = identificationType;
        return this;
    }
    
    public FakeDriverBuilder WithIdentificationNumber(string identificationNumber)
    {
        _creationData.IdentificationNumber = identificationNumber;
        return this;
    }
    
    public FakeDriverBuilder WithIdentificationExpirationDate(string identificationExpirationDate)
    {
        _creationData.IdentificationExpirationDate = identificationExpirationDate;
        return this;
    }
    
    public FakeDriverBuilder WithAddress(string address)
    {
        _creationData.Address = address;
        return this;
    }
    
    public FakeDriverBuilder WithPhone(string phone)
    {
        _creationData.Phone = phone;
        return this;
    }
    
    public FakeDriverBuilder WithEmail(string email)
    {
        _creationData.Email = email;
        return this;
    }
    
    public FakeDriverBuilder WithDriverLicenseNumber(string driverLicenseNumber)
    {
        _creationData.DriverLicenseNumber = driverLicenseNumber;
        return this;
    }
    
    public FakeDriverBuilder WithDriverLicenseCategory(string driverLicenseCategory)
    {
        _creationData.DriverLicenseCategory = driverLicenseCategory;
        return this;
    }
    
    public FakeDriverBuilder WithDriverLicenseIssuingDate(string driverLicenseIssuingDate)
    {
        _creationData.DriverLicenseIssuingDate = driverLicenseIssuingDate;
        return this;
    }
    
    public FakeDriverBuilder WithDriverLicenseExpirationDate(string driverLicenseExpirationDate)
    {
        _creationData.DriverLicenseExpirationDate = driverLicenseExpirationDate;
        return this;
    }
    
    public FakeDriverBuilder WithDriverLicenseIssuingAuthority(string driverLicenseIssuingAuthority)
    {
        _creationData.DriverLicenseIssuingAuthority = driverLicenseIssuingAuthority;
        return this;
    }
    
    public FakeDriverBuilder WithEmploymentStatus(string employmentStatus)
    {
        _creationData.EmploymentStatus = employmentStatus;
        return this;
    }
    
    public FakeDriverBuilder WithEmploymentStartDate(string employmentStartDate)
    {
        _creationData.EmploymentStartDate = employmentStartDate;
        return this;
    }
    
    public FakeDriverBuilder WithEmploymentEndDate(string employmentEndDate)
    {
        _creationData.EmploymentEndDate = employmentEndDate;
        return this;
    }
    
    public Driver Build()
    {
        var result = Driver.Create(_creationData);
        return result;
    }
}