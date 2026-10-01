namespace Drivers.DAL.ADO.Entities.HelpModels;

/// <summary>
/// Сповіщення про закінчення терміну дії водійського посвідчення (ADO.NET / Dapper)
/// </summary>
public class DriverLicenseAlert
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
