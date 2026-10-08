namespace CargoDelivery.HttpAggregator.Domain.Dtos;

/// <summary>
/// Агреговані дані водія, отримані з мікросервісу DriversManagement.
/// </summary>
public sealed class AggregatedDriverDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
}
