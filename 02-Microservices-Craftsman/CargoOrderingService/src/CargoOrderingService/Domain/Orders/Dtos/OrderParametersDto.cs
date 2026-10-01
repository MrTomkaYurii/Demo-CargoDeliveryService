namespace CargoOrderingService.Domain.Orders.Dtos;

using CargoOrderingService.Resources;

public sealed class OrderParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
