namespace DriversManagement.Domain.Drivers.Dtos;

using Destructurama.Attributed;

public sealed record DriverForCreationDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string MiddleName { get; set; }
    public string Gender { get; set; }
    public string Birthdate { get; set; }
    public string PlaceOfBirth { get; set; }
    public string Nationality { get; set; }
    public string MaritalStatus { get; set; }
    public string IdentificationType { get; set; }
    public string IdentificationNumber { get; set; }
    public string IdentificationExpirationDate { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public string DriverLicenseNumber { get; set; }
    public string DriverLicenseCategory { get; set; }
    public string DriverLicenseIssuingDate { get; set; }
    public string DriverLicenseExpirationDate { get; set; }
    public string DriverLicenseIssuingAuthority { get; set; }
    public string EmploymentStatus { get; set; }
    public string EmploymentStartDate { get; set; }
    public string EmploymentEndDate { get; set; }

}
