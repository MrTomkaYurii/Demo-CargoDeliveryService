namespace Drivers.BLL.DTOs.Requests;

/// <summary>
/// DTO запиту для створення або оновлення витрат
/// </summary>
public class ExpenseReqDTO
{
    public decimal DriverPayment { get; set; }
    public decimal FuelCost { get; set; }
    public decimal MaintenanceCost { get; set; }
    public string Category { get; set; } = default!;
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public int DriverId { get; set; }
    public int TruckId { get; set; }
}
