namespace Drivers.BLL.DTOs.Responses;

/// <summary>
/// DTO відповіді з інформацією про витрати
/// </summary>
public class ExpenseResponceDTO
{
    public int Id { get; set; }
    public decimal DriverPayment { get; set; }
    public decimal FuelCost { get; set; }
    public decimal MaintenanceCost { get; set; }
    public decimal TotalExpense => DriverPayment + FuelCost + MaintenanceCost;
    public string Category { get; set; } = default!;
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public int DriverId { get; set; }
    public int TruckId { get; set; }
    public TruckResponceDTO? Truck { get; set; }
}
