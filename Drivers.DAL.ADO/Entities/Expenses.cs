namespace Drivers.DAL.ADO.Entities;

/// <summary>
/// Сутність витрат (ADO.NET / Dapper).
/// Відповідає структурі таблиці [dbo].[Expenses].
/// </summary>
public class Expenses
{
    public int Id { get; set; }
    public decimal DriverPayment { get; set; }
    public decimal FuelCost { get; set; }
    public decimal MaintenanceCost { get; set; }
    public string Category { get; set; } = default!;
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public int DriverID { get; set; }
    public int TruckID { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Driver? Driver { get; set; }
    public Truck? Truck { get; set; }
}
