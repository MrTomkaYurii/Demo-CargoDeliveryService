namespace DriversManagement.Domain.Trucks.Dtos;

using DriversManagement.Resources;

public sealed class TruckParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
