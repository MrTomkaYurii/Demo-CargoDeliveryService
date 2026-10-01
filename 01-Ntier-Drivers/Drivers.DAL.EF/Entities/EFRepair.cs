namespace Drivers.DAL.EF.Entities;

public class EFRepair
{
    public int Id { get; set; }
    public DateTime RepairDate { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public int? TruckId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual EFTruck? Truck { get; set; }
}
