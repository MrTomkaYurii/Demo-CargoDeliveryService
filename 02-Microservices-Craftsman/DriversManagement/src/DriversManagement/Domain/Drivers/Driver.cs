using DriversManagement.Domain.Expences;
using DriversManagement.Domain.Companies;
using DriversManagement.Domain.Companies.Models;
using DriversManagement.Domain.Photos;
using DriversManagement.Domain.Photos.Models;
using DriversManagement.Domain.Drivers.Models;
using DriversManagement.Domain.Drivers.DomainEvents;

namespace DriversManagement.Domain.Drivers;
public class Driver : BaseEntity
{
    public string FirstName { get; private set; }

    public string LastName { get; private set; }

    public string MiddleName { get; private set; }

    public string Gender { get; private set; }

    public string Birthdate { get; private set; }

    public string PlaceOfBirth { get; private set; }

    public string Nationality { get; private set; }

    public string MaritalStatus { get; private set; }

    public string IdentificationType { get; private set; }

    public string IdentificationNumber { get; private set; }

    public string IdentificationExpirationDate { get; private set; }

    public string Address { get; private set; }

    public string Phone { get; private set; }

    public string Email { get; private set; }

    public string DriverLicenseNumber { get; private set; }

    public string DriverLicenseCategory { get; private set; }

    public string DriverLicenseIssuingDate { get; private set; }

    public string DriverLicenseExpirationDate { get; private set; }

    public string DriverLicenseIssuingAuthority { get; private set; }

    public string EmploymentStatus { get; private set; }

    public string EmploymentStartDate { get; private set; }

    public string EmploymentEndDate { get; private set; }

    public Photo Photo { get; private set; } = Photo.Create(new PhotoForCreation());

    public Company Company { get; private set; } = Company.Create(new CompanyForCreation());

    public IReadOnlyCollection<Expence> Expences { get; } = new List<Expence>();

    // Add Props Marker -- Deleting this comment will cause the add props utility to be incomplete


    public static Driver Create(DriverForCreation driverForCreation)
    {
        var newDriver = new Driver();

        newDriver.FirstName = driverForCreation.FirstName;
        newDriver.LastName = driverForCreation.LastName;
        newDriver.MiddleName = driverForCreation.MiddleName;
        newDriver.Gender = driverForCreation.Gender;
        newDriver.Birthdate = driverForCreation.Birthdate;
        newDriver.PlaceOfBirth = driverForCreation.PlaceOfBirth;
        newDriver.Nationality = driverForCreation.Nationality;
        newDriver.MaritalStatus = driverForCreation.MaritalStatus;
        newDriver.IdentificationType = driverForCreation.IdentificationType;
        newDriver.IdentificationNumber = driverForCreation.IdentificationNumber;
        newDriver.IdentificationExpirationDate = driverForCreation.IdentificationExpirationDate;
        newDriver.Address = driverForCreation.Address;
        newDriver.Phone = driverForCreation.Phone;
        newDriver.Email = driverForCreation.Email;
        newDriver.DriverLicenseNumber = driverForCreation.DriverLicenseNumber;
        newDriver.DriverLicenseCategory = driverForCreation.DriverLicenseCategory;
        newDriver.DriverLicenseIssuingDate = driverForCreation.DriverLicenseIssuingDate;
        newDriver.DriverLicenseExpirationDate = driverForCreation.DriverLicenseExpirationDate;
        newDriver.DriverLicenseIssuingAuthority = driverForCreation.DriverLicenseIssuingAuthority;
        newDriver.EmploymentStatus = driverForCreation.EmploymentStatus;
        newDriver.EmploymentStartDate = driverForCreation.EmploymentStartDate;
        newDriver.EmploymentEndDate = driverForCreation.EmploymentEndDate;

        newDriver.QueueDomainEvent(new DriverCreated(){ Driver = newDriver });
        
        return newDriver;
    }

    public Driver Update(DriverForUpdate driverForUpdate)
    {
        FirstName = driverForUpdate.FirstName;
        LastName = driverForUpdate.LastName;
        MiddleName = driverForUpdate.MiddleName;
        Gender = driverForUpdate.Gender;
        Birthdate = driverForUpdate.Birthdate;
        PlaceOfBirth = driverForUpdate.PlaceOfBirth;
        Nationality = driverForUpdate.Nationality;
        MaritalStatus = driverForUpdate.MaritalStatus;
        IdentificationType = driverForUpdate.IdentificationType;
        IdentificationNumber = driverForUpdate.IdentificationNumber;
        IdentificationExpirationDate = driverForUpdate.IdentificationExpirationDate;
        Address = driverForUpdate.Address;
        Phone = driverForUpdate.Phone;
        Email = driverForUpdate.Email;
        DriverLicenseNumber = driverForUpdate.DriverLicenseNumber;
        DriverLicenseCategory = driverForUpdate.DriverLicenseCategory;
        DriverLicenseIssuingDate = driverForUpdate.DriverLicenseIssuingDate;
        DriverLicenseExpirationDate = driverForUpdate.DriverLicenseExpirationDate;
        DriverLicenseIssuingAuthority = driverForUpdate.DriverLicenseIssuingAuthority;
        EmploymentStatus = driverForUpdate.EmploymentStatus;
        EmploymentStartDate = driverForUpdate.EmploymentStartDate;
        EmploymentEndDate = driverForUpdate.EmploymentEndDate;

        QueueDomainEvent(new DriverUpdated(){ Id = Id });
        return this;
    }

    // Add Prop Methods Marker -- Deleting this comment will cause the add props utility to be incomplete
    
    protected Driver() { } // For EF + Mocking
}
