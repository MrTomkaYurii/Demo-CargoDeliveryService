namespace Drivers.BLL.DTOs.Requests;

/// <summary>
/// DTO запиту для створення або оновлення техогляду
/// </summary>
public class InspectionReqDTO
{
    public DateTime InspectionDate { get; set; }
    public string? Description { get; set; }
    public bool Result { get; set; }
    public int? TruckId { get; set; }
}
