namespace Drivers.DAL.ADO.Entities;

/// <summary>
/// Сутність водія для шару доступу до даних ADO.NET / Dapper.
/// Відповідає структурі таблиці [dbo].[Drivers].
/// </summary>
public class Driver
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? MiddleName { get; set; }
    public string Gender { get; set; } = default!;
    public DateTime Birthdate { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? Nationality { get; set; }
    public string? MaritalStatus { get; set; }
    public string? IdentificationType { get; set; }
    public string? IdentificationNumber { get; set; }
    public DateTime? IdentificationExpirationDate { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string DriverLicenseNumber { get; set; } = default!;
    public string? DriverLicenseCategory { get; set; }
    public DateTime DriverLicenseIssuingDate { get; set; }
    public DateTime DriverLicenseExpirationDate { get; set; }
    public string? DriverLicenseIssuingAuthority { get; set; }
    public string? EmploymentStatus { get; set; }
    public DateTime? EmploymentStartDate { get; set; }
    public DateTime? EmploymentEndDate { get; set; }

    public int? CompanyID { get; set; }
    public Company? Company { get; set; }
    public int? PhotoID { get; set; }
    public Photo? Photo { get; set; }

    public List<Expenses> Expenses { get; set; } = new();

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
