namespace Drivers.BLL.DTOs.Responses;

/// <summary>
/// DTO відповіді зі сповіщенням про закінчення терміну дії водійського посвідчення
/// </summary>
public class DriverLicenseAlertResponseDTO
{
    public int DriverId { get; set; }
    public string FullName { get; set; } = default!;
    public string DriverLicenseNumber { get; set; } = default!;
    public string? DriverLicenseCategory { get; set; }
    public DateTime DriverLicenseExpirationDate { get; set; }
    public int DaysUntilExpiration { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
