using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Specifications;

namespace Drivers.DAL.EF.Specifications;

/// <summary>
/// Специфікація пошуку, комбінованої фільтрації та пагінації водіїв.
/// <para>
/// <b>Бізнес-призначення:</b> Інкапсулює складний набір критеріїв пошуку:
/// частковий збіг за прізвищем, статус зайнятості, діапазон року народження,
/// жадібне завантаження пов'язаних компанії та фотографії, сортування та сторінкову вибірку.
/// </para>
/// <para>
/// <b>Як працює динамічний Expression:</b> Умова <c>base(...)</c> використовує логічний оператор <c>||</c>
/// для перевірки на <c>null</c> або порожній рядок. Entity Framework Core транслює такі перевірки 
/// у T-SQL предикати, де неактивні параметри автоматично спрощуються в <c>1 = 1</c>,
/// а активні перетворюються на відповідні <c>WHERE [LastName] LIKE ...</c> та <c>[Birthdate] &gt;= ...</c>.
/// </para>
/// </summary>
public class DriverFilterSpecification : BaseSpecification<EFDriver>
{
    /// <summary>
    /// Ініціалізує нову бізнес-специфікацію фільтрації водіїв.
    /// </summary>
    /// <param name="searchLastName">Фрагмент прізвища водія для пошуку (опціонально)</param>
    /// <param name="employmentStatus">Статус працевлаштування (Active, OnLeave, Inactive)</param>
    /// <param name="minBirthYear">Мінімальний рік народження (включно)</param>
    /// <param name="maxBirthYear">Максимальний рік народження (включно)</param>
    /// <param name="skip">Кількість записів для пропуску (Offset для пагінації)</param>
    /// <param name="take">Кількість записів на сторінку (Limit)</param>
    /// <param name="isPaging">Прапорець активації серверної пагінації</param>
    public DriverFilterSpecification(
        string? searchLastName = null,
        string? employmentStatus = null,
        int? minBirthYear = null,
        int? maxBirthYear = null,
        int skip = 0,
        int take = 10,
        bool isPaging = false)
        : base(driver =>
            // 1. Пошук за прізвищем: якщо searchLastName порожнє, умова ігнорується
            (string.IsNullOrWhiteSpace(searchLastName) || driver.LastName.Contains(searchLastName)) &&
            // 2. Фільтр за статусом: якщо employmentStatus порожнє, умова ігнорується
            (string.IsNullOrWhiteSpace(employmentStatus) || driver.EmploymentStatus == employmentStatus) &&
            // 3. Діапазон років народження: якщо параметр null, умова ігнорується
            (!minBirthYear.HasValue || driver.Birthdate.Year >= minBirthYear.Value) &&
            (!maxBirthYear.HasValue || driver.Birthdate.Year <= maxBirthYear.Value))
    {
        // 1. Eager Loading (жадібне завантаження зв'язаних даних у рамках єдиного SQL-запиту)
        AddInclude(d => d.Company);
        AddInclude(d => d.Photo);

        // 2. Сортування за замовчуванням: за алфавітом прізвища (ORDER BY [d].[LastName] ASC)
        ApplyOrderBy(d => d.LastName);

        // 3. Серверна пагінація: додає OFFSET/FETCH до SQL-запиту
        if (isPaging)
        {
            ApplyPaging(skip, take);
        }
    }
}
