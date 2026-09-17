namespace Drivers.DAL.ADO.Entities.HelpModels;

/// <summary>
/// Параметри фільтрації, пагінації та сортування водіїв для ADO.NET / Dapper.
/// </summary>
public class DriverParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>
    /// Поле сортування (наприклад: "LastName", "LastName desc", "Birthdate asc")
    /// </summary>
    public string OrderBy { get; set; } = "LastName";

    /// <summary>
    /// Мінімальний рік народження для фільтрації
    /// </summary>
    public uint MinYearOfBirth { get; set; }

    /// <summary>
    /// Максимальний рік народження для фільтрації
    /// </summary>
    public uint MaxYearOfBirth { get; set; } = (uint)DateTime.Now.Year;

    public bool ValidYearRange => MaxYearOfBirth >= MinYearOfBirth;

    /// <summary>
    /// Пошук за прізвищем водія
    /// </summary>
    public string? LastName { get; set; }
}
