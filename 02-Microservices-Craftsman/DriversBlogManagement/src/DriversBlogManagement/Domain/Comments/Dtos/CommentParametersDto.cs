namespace DriversBlogManagement.Domain.Comments.Dtos;

using DriversBlogManagement.Resources;

public sealed class CommentParametersDto : BasePaginationParameters
{
    public string? Filters { get; set; }
    public string? SortOrder { get; set; }
}
