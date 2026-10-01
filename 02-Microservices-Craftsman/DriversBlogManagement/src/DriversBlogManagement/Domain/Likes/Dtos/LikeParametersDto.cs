namespace DriversBlogManagement.Domain.Likes.Dtos;

using DriversBlogManagement.Resources;

public sealed class LikeParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
