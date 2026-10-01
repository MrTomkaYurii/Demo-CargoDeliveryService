namespace DriversBlogManagement.Domain.BlogUsers.Dtos;

using DriversBlogManagement.Resources;

public sealed class BlogUserParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
