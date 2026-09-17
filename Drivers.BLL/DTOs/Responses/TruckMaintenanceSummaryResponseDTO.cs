namespace Drivers.BLL.DTOs.Responses;

/// <summary>
/// DTO відповіді з аналітикою технічного обслуговування вантажівки
/// </summary>
public class TruckMaintenanceSummaryResponseDTO
{
    public int TruckId { get; set; }
    public string TruckNumber { get; set; } = default!;
    public string Model { get; set; } = default!;
    public decimal TotalMaintenanceCost { get; set; }
    public decimal TotalRepairsCost { get; set; }
    public decimal OverallServiceCost { get; set; }
    public int TotalInspectionsCount { get; set; }
    public int PassedInspectionsCount { get; set; }
    public int FailedInspectionsCount { get; set; }
}
