using Drivers.DAL.EF.Specifications.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Drivers.DAL.EF.Specifications.Evaluator;

/// <summary>
/// Оцінювач специфікацій (Specification Evaluator).
/// Відповідає за трансляцію об'єкта ISpecification у фінальний конвеєр запиту IQueryable для Entity Framework Core.
/// Динамічно компонує Where, Include, OrderBy, Skip та Take без витоку деталей запиту у вищі шари.
/// </summary>
/// <typeparam name="TEntity">Тип сутності</typeparam>
public static class SpecificationEvaluator<TEntity> where TEntity : class
{
    /// <summary>
    /// Побудова IQueryable на основі переданої специфікації
    /// </summary>
    /// <param name="inputQuery">Базовий набір даних DbSet або IQueryable</param>
    /// <param name="specification">Специфікація з критеріями запиту</param>
    /// <returns>Готовий до виконання IQueryable</returns>
    public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> specification)
    {
        var query = inputQuery;

        // 1. Застосування умови фільтрації (WHERE)
        if (specification.Criteria != null)
        {
            query = query.Where(specification.Criteria);
        }

        // 2. Жадібне завантаження через сильно типізовані Expressions (.Include)
        query = specification.Includes.Aggregate(query,
            (current, include) => current.Include(include));

        // 3. Жадібне завантаження через рядкові шляхи (для глибоких вкладених зв'язків)
        query = specification.IncludeStrings.Aggregate(query,
            (current, include) => current.Include(include));

        // 4. Сортування
        if (specification.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy);
        }
        else if (specification.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending);
        }

        // 5. Пагінація (Skip / Take)
        if (specification.IsPagingEnabled)
        {
            query = query.Skip(specification.Skip).Take(specification.Take);
        }

        return query;
    }
}
