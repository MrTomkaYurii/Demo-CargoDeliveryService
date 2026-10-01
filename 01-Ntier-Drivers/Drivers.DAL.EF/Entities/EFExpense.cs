namespace Drivers.DAL.EF.Entities;

public partial class EFExpense
{
    public int Id { get; set; }
    public int DriverId { get; set; }
    public int TruckId { get; set; }
    public decimal DriverPayment { get; set; }
    public decimal FuelCost { get; set; }
    public decimal MaintenanceCost { get; set; }
    public string Category { get; set; } = null!;
    public DateTime Date { get; set; }
    public string? Note { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual EFDriver Driver { get; set; } = null!;
    public virtual EFTruck Truck { get; set; } = null!;
}
