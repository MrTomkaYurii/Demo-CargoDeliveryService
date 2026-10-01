namespace Drivers.BLL.DTOs.Requests;

/// <summary>
/// DTO запиту для створення або оновлення запису про ремонт
/// </summary>
public class RepairReqDTO
{
    public DateTime RepairDate { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public int? TruckId { get; set; }
}
