namespace Drivers.BLL.DTOs.Responses;

/// <summary>
/// DTO відповіді компанії разом зі списком закріплених водіїв
/// </summary>
public class CompanyWithDriversResponseDTO : CompanyResponceDTO
{
    public List<ShortDriverResponceDTO> Drivers { get; set; } = new();
}
