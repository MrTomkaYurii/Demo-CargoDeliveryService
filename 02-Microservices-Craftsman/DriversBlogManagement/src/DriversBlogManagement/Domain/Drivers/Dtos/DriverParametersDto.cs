namespace DriversBlogManagement.Domain.Drivers.Dtos;

using DriversBlogManagement.Resources;

public sealed class DriverParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
