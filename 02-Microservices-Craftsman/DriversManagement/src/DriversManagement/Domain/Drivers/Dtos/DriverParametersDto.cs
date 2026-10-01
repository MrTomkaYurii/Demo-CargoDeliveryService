namespace DriversManagement.Domain.Drivers.Dtos;

using DriversManagement.Resources;

public sealed class DriverParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
