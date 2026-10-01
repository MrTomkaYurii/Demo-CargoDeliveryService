namespace CargoOrderingService.Domain.Deliveries.Dtos;

using CargoOrderingService.Resources;

public sealed class DeliveryParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
