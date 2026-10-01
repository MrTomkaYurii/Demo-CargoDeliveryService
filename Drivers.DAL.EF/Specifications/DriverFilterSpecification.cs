using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Specifications;

namespace Drivers.DAL.EF.Specifications;

/// <summary>
/// Специфікація пошуку та фільтрації водіїв.
/// Демонструє інкапсуляцію комбінованих бізнес-критеріїв:
/// фільтрація за прізвищем, статусом працевлаштування, мінімальним/максимальним роком народження,
/// сортування та включення зв'язаних даних.
/// </summary>
public class DriverFilterSpecification : BaseSpecification<EFDriver>
{
    public DriverFilterSpecification(
        string? searchLastName = null,
        string? employmentStatus = null,
        int? minBirthYear = null,
        int? maxBirthYear = null,
        int skip = 0,
        int take = 10,
        bool isPaging = false)
        : base(driver =>
            (string.IsNullOrWhiteSpace(searchLastName) || driver.LastName.Contains(searchLastName)) &&
            (string.IsNullOrWhiteSpace(employmentStatus) || driver.EmploymentStatus == employmentStatus) &&
            (!minBirthYear.HasValue || driver.Birthdate.Year >= minBirthYear.Value) &&
            (!maxBirthYear.HasValue || driver.Birthdate.Year <= maxBirthYear.Value))
    {
        // Включаємо фото та компанію
        AddInclude(d => d.Company);
        AddInclude(d => d.Photo);

        // Сортування за замовчуванням
        ApplyOrderBy(d => d.LastName);

        // Пагінація, якщо активована
        if (isPaging)
        {
            ApplyPaging(skip, take);
        }
    }
}
