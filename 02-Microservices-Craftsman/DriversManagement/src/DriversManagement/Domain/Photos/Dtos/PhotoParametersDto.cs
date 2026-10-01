namespace DriversManagement.Domain.Photos.Dtos;

using DriversManagement.Resources;

public sealed class PhotoParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
