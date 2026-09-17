namespace Drivers.BLL.DTOs.Responses;

/// <summary>
/// Універсальна DTO-модель пагінованої відповіді з метаданими сторінок
/// </summary>
/// <typeparam name="T">Тип елементів</typeparam>
public class PagedResponseDTO<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}
