namespace Drivers.BLL.DTOs.Requests;

/// <summary>
/// DTO запиту для створення або оновлення компанії
/// </summary>
public class CompanyReqDTO
{
    public string CompanyName { get; set; } = default!;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
}
