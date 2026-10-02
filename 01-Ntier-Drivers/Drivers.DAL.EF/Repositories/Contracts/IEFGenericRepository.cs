using System.Linq.Expressions;
using Drivers.DAL.EF.Specifications.Contracts;

namespace Drivers.DAL.EF.Repositories.Contracts;

/// <summary>
/// Базовий інтерфейс узагальненого репозиторію для EF Core.
/// <para>
/// Усі асинхронні операції приймають <see cref="CancellationToken"/> для забезпечення наскрізного скасування
/// тривалих або непотрібних SQL-запитів на рівні бази даних (запобігання "примарним запитам" / Ghost Queries).
/// </para>
/// </summary>
/// <typeparam name="TEntity">Тип сутності бази даних</typeparam>
public interface IEFGenericRepository<TEntity> where TEntity : class
{
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TEntity> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<TEntity?> GetCompleteEntityAsync(int id, CancellationToken cancellationToken = default);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(TEntity entity);

    Task DeleteByIdAsync(int id, CancellationToken cancellationToken = default);

    Task DeleteAsync(TEntity entity);

    // Filters
    IQueryable<TEntity> FindAll();

    Task<IQueryable<TEntity>> FindByCondition(Expression<Func<TEntity, bool>> expression);

    // Specification Pattern
    Task<IReadOnlyList<TEntity>> GetWithSpecAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    Task<TEntity?> GetEntityWithSpecAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);

    Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default);
}
