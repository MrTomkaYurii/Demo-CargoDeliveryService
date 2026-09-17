namespace Drivers.BLL.DTOs.Responses;

/// <summary>
/// DTO відповіді для техогляду
/// </summary>
public class InspectionResponceDTO
{
    public int Id { get; set; }
    public DateTime InspectionDate { get; set; }
    public string? Description { get; set; }
    public bool Result { get; set; }
    public int? TruckId { get; set; }
    public TruckResponceDTO? Truck { get; set; }
}
