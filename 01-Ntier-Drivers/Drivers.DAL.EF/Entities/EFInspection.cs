namespace Drivers.DAL.EF.Entities;

public class EFInspection
{
    public int Id { get; set; }
    public DateTime InspectionDate { get; set; }
    public string? Description { get; set; }
    public bool Result { get; set; }
    public int? TruckId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual EFTruck? Truck { get; set; }
}
