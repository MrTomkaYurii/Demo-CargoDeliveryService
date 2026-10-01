using System.Linq.Expressions;

namespace Drivers.DAL.EF.Specifications.Contracts;

/// <summary>
/// Базовий контракт патерну Specification (Специфікація).
/// Інкапсулює критерії фільтрації, зв'язування таблиць (Includes), сортування та пагінацію
/// в єдиний перевикористовуваний об'єкт запиту, усуваючи дублювання та розростання методів у репозиторіях.
/// </summary>
/// <typeparam name="T">Тип сутності бази даних</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Головний предикат фільтрації (WHERE умова)
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Список виразів жадібного завантаження навігаційних зв'язків (Eager Loading через .Include())
    /// </summary>
    List<Expression<Func<T, object?>>> Includes { get; }

    /// <summary>
    /// Список рядкових шляхів для жадібного завантаження (вкладені зв'язки типу "Expenses.Truck")
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Вираз сортування за зростанням (ORDER BY ... ASC)
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Вираз сортування за спаданням (ORDER BY ... DESC)
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Кількість записів для вибірки (LIMIT / TAKE)
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Кількість записів для пропуску (OFFSET / SKIP)
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Прапорець активації механізму пагінації
    /// </summary>
    bool IsPagingEnabled { get; }
}
