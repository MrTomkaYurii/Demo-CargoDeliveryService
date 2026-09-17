namespace Drivers.DAL.ADO.Entities.HelpModels;

/// <summary>
/// Зведена інформація про технічне обслуговування, ремонти та інспекції вантажівки (ADO.NET / Dapper)
/// </summary>
public class TruckMaintenanceSummary
{
    public int TruckId { get; set; }
    public string TruckNumber { get; set; } = default!;
    public string Model { get; set; } = default!;
    public decimal TotalMaintenanceCost { get; set; }
    public decimal TotalRepairsCost { get; set; }
    public decimal OverallServiceCost => TotalMaintenanceCost + TotalRepairsCost;
    public int TotalInspectionsCount { get; set; }
    public int PassedInspectionsCount { get; set; }
    public int FailedInspectionsCount { get; set; }
}
