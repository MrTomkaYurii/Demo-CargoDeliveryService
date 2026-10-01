using Drivers.DAL.EF.Specifications.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Drivers.DAL.EF.Specifications.Evaluator;

/// <summary>
/// Оцінювач та компілятор специфікацій (Specification Evaluator).
/// <para>
/// <b>Архітектурна роль:</b> Виступає сполучною ланкою між доменною специфікацією (<see cref="ISpecification{T}"/>) 
/// та конвеєром побудови запиту <see cref="IQueryable{T}"/> в Entity Framework Core.
/// </para>
/// <para>
/// <b>Принцип роботи:</b> Приймає базовий набір даних DbSet та послідовно накладає на нього 
/// дерево виразів фільтрації (<c>.Where()</c>), зв'язки жадібного завантаження (<c>.Include()</c>), 
/// правила сортування (<c>.OrderBy()</c>) та серверну пагінацію (<c>.Skip().Take()</c>).
/// </para>
/// </summary>
/// <typeparam name="TEntity">Тип сутності бази даних</typeparam>
public static class SpecificationEvaluator<TEntity> where TEntity : class
{
    /// <summary>
    /// Будує та конфігурує фінальний <see cref="IQueryable{TEntity}"/> запит на основі переданої специфікації.
    /// </summary>
    /// <param name="inputQuery">Базовий набір даних (DbSet або AsNoTracking запит)</param>
    /// <param name="specification">Об'єкт специфікації з налаштуваннями запиту</param>
    /// <returns>Скомпонований IQueryable, готовий до трансляції в SQL Server T-SQL</returns>
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> specification)
    {
        var query = inputQuery;

        // 1. Застосування умови фільтрації (WHERE предикат)
        // Expression<Func<T, bool>> передається в Where, де Query Provider розбирає AST-вузли на T-SQL WHERE
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // 2. Жадібне завантаження (Eager Loading) через типізовані Expression дерева (.Include)
        // Використовуємо функціональну згортку Aggregate, що послідовно огортає query:
        // query.Include(expr1).Include(expr2)...
        query = specification.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        // 3. Жадібне завантаження через рядкові шляхи (для вкладених зв'язків типу "Expenses.Truck")
        query = specification.IncludeStrings.Aggregate(query,
            (current, include) => current.Include(include));

        // 4. Сортування результату (ORDER BY)
        // Встановлюється вираз сортування за зростанням або спаданням
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // 5. Серверна пагінація (Skip / Take)
        // Перетворюється на T-SQL: OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
        // Важливо: пагінація завжди має застосовуватися ПІСЛЯ сортування!
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }
}
