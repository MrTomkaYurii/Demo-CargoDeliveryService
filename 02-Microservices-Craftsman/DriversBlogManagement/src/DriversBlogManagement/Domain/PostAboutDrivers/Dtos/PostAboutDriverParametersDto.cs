namespace DriversBlogManagement.Domain.PostAboutDrivers.Dtos;

using DriversBlogManagement.Resources;

public sealed class PostAboutDriverParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
