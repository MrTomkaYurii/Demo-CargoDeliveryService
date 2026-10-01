using System.Linq.Expressions;

namespace Drivers.DAL.EF.Specifications.Contracts;

/// <summary>
/// Базовий контракт патерну Specification (Специфікація).
/// <para>
/// <b>Призначення:</b> Інкапсулює критерії фільтрації, зв'язування таблиць (Includes), сортування та пагінацію
/// в єдиний перевикористовуваний об'єкт запиту, усуваючи "товсті репозиторії" (Fat Repository) 
/// та "витік деталей збереження" (Leaky Abstraction).
/// </para>
/// <para>
/// <b>Теорія Expression Trees:</b> Усі критерії передаються як <see cref="Expression{TDelegate}"/>,
/// а не як звичайні делегати <see cref="Func{T, TResult}"/>. Завдяки цьому Entity Framework Core
/// отримує не скомпільований IL-байткод, а синтаксичне дерево (AST), яке Query Provider здатний
/// розпарсити та транслювати в оптимальний T-SQL запит на стороні сервера БД.
/// </para>
/// </summary>
/// <typeparam name="T">Тип сутності бази даних</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Головний предикат фільтрації (WHERE умова).
    /// Представлений як дерево виразів <see cref="Expression{Func}"/>, що перекладається в T-SQL <c>WHERE</c>.
    /// </summary>
    Expression<Func<T, bool>>? Criteria { get; }

    /// <summary>
    /// Список дерев виразів для жадібного завантаження навігаційних зв'язків (Eager Loading через .Include()).
    /// Сильна типізація гарантує перевірку наявності навігаційних властивостей ще на етапі компіляції.
    /// </summary>
    List<Expression<Func<T, object?>>> Includes { get; }

    /// <summary>
    /// Список рядкових шляхів для жадібного завантаження глибоких вкладених зв'язків (наприклад, "Expenses.Truck").
    /// </summary>
    List<string> IncludeStrings { get; }

    /// <summary>
    /// Дерево виразів для сортування за зростанням (перекладається в T-SQL <c>ORDER BY ... ASC</c>).
    /// </summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>
    /// Дерево виразів для сортування за спаданням (перекладається в T-SQL <c>ORDER BY ... DESC</c>).
    /// </summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>
    /// Кількість записів для вибірки (перекладається в T-SQL <c>FETCH NEXT @Take ROWS ONLY</c>).
    /// </summary>
    int Take { get; }

    /// <summary>
    /// Кількість записів для пропуску (перекладається в T-SQL <c>OFFSET @Skip ROWS</c>).
    /// </summary>
    int Skip { get; }

    /// <summary>
    /// Прапорець активації механізму серверної пагінації.
    /// </summary>
    bool IsPagingEnabled { get; }
}
