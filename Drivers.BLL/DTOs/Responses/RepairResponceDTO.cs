namespace Drivers.BLL.DTOs.Responses;

/// <summary>
/// DTO відповіді для ремонту
/// </summary>
public class RepairResponceDTO
{
    public int Id { get; set; }
    public DateTime RepairDate { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public int? TruckId { get; set; }
    public TruckResponceDTO? Truck { get; set; }
}
