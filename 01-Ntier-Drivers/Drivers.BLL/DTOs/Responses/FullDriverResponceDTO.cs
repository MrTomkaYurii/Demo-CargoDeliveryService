namespace Drivers.BLL.DTOs.Responses;

/// <summary>
/// Повна детальна інформація про водія з навігаційними даними (компанія, фото, витрати)
/// </summary>
public class FullDriverResponceDTO
{
    public int? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public string? Gender { get; set; }
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
    public string? DriverLicenseNumber { get; set; }
    public string? DriverLicenseCategory { get; set; }
    public DateTime? DriverLicenseIssuingDate { get; set; }
    public DateTime? DriverLicenseExpirationDate { get; set; }
    public string? DriverLicenseIssuingAuthority { get; set; }
    public string? EmploymentStatus { get; set; }
    public DateTime? EmploymentStartDate { get; set; }
    public DateTime? EmploymentEndDate { get; set; }
    public int? CompanyId { get; set; }
    public int? PhotoId { get; set; }

    public CompanyResponceDTO? Company { get; set; }
    public PhotoResponceDTO? Photo { get; set; }
    public ICollection<ExpenseResponceDTO>? Expenses { get; set; }
}
