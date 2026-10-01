using Drivers.DAL.EF.Data;
using Drivers.DAL.EF.Exceptions;
using Drivers.DAL.EF.Repositories.Contracts;
using Drivers.DAL.EF.Specifications.Contracts;
using Drivers.DAL.EF.Specifications.Evaluator;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Drivers.DAL.EF.Repositories;

/// <summary>
/// Базовий узагальнений репозиторій для Entity Framework Core.
/// Забезпечує стандартні CRUD-операції з DbContext.
/// </summary>
/// <typeparam name="TEntity">Тип сутності</typeparam>
public abstract class EFGenericRepository<TEntity> : IEFGenericRepository<TEntity> where TEntity : class
{
    protected readonly DriversManagementContext databaseContext;
    protected readonly DbSet<TEntity> table;

    public EFGenericRepository(DriversManagementContext databaseContext)
    {
        this.databaseContext = databaseContext;
        table = this.databaseContext.Set<TEntity>();
    }

    /// <summary>
    /// Отримання сутності за Id
    /// </summary>
    public virtual async Task<TEntity> GetByIdAsync(int id)
    {
        return await table.FindAsync(id)
            ?? throw new EntityNotFoundException($"{typeof(TEntity).Name} з id {id} не знайдено.");
    }

    /// <summary>
    /// Отримання всіх сутностей
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await table.ToListAsync();
    }

    /// <summary>
    /// Додавання нової сутності
    /// </summary>
    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), $"{typeof(TEntity).Name} entity не може бути null");
        }

        var addedEntity = await table.AddAsync(entity);
        return addedEntity.Entity;
    }

    /// <summary>
    /// Оновлення сутності
    /// </summary>
    public virtual async Task UpdateAsync(TEntity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), $"{typeof(TEntity).Name} entity не може бути null");
        }
        await Task.Run(() => table.Update(entity));
    }

    /// <summary>
    /// Видалення сутності за Id
    /// </summary>
    public virtual async Task DeleteByIdAsync(int id)
    {
        var entity = await table.FindAsync(id)
            ?? throw new EntityNotFoundException($"{typeof(TEntity).Name} з id {id} не знайдено. Неможливо видалити.");
        await Task.Run(() => table.Remove(entity));
    }

    /// <summary>
    /// Видалення переданої сутності
    /// </summary>
    public virtual async Task DeleteAsync(TEntity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), $"{typeof(TEntity).Name} entity не може бути null");
        }
        await Task.Run(() => table.Remove(entity));
    }

    /// <summary>
    /// Отримання IQueryable без відстеження змін
    /// </summary>
    public IQueryable<TEntity> FindAll()
    {
        return databaseContext.Set<TEntity>().AsNoTracking();
    }

    /// <summary>
    /// Пошук сутностей за предикатом без відстеження змін
    /// </summary>
    public async Task<IQueryable<TEntity>> FindByCondition(Expression<Func<TEntity, bool>> expression)
    {
        return await Task.Run(() => databaseContext.Set<TEntity>()
            .Where(expression)
            .AsNoTracking());
    }

    /// <summary>
    /// Отримання повної сутності разом із пов'язаними навігаційними властивостями
    /// </summary>
    public abstract Task<TEntity?> GetCompleteEntityAsync(int id);

    /// <summary>
    /// Вибірка списку сутностей за об'єктом специфікації (WHERE, Includes, OrderBy, Paging)
    /// </summary>
    public virtual async Task<IReadOnlyList<TEntity>> GetWithSpecAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Отримання одиничної сутності за об'єктом специфікації
    /// </summary>
    public virtual async Task<TEntity?> GetEntityWithSpecAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Підрахунок кількості сутностей, що відповідають критеріям специфікації
    /// </summary>
    public virtual async Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    /// <summary>
    /// Допоміжний метод підключення SpecificationEvaluator до поточного набору даних
    /// </summary>
    protected IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
    {
        return SpecificationEvaluator<TEntity>.GetQuery(table.AsNoTracking(), specification);
    }
}
