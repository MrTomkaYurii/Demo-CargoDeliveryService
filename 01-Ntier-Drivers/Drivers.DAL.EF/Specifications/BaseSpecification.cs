using Drivers.DAL.EF.Specifications.Contracts;
using System.Linq.Expressions;

namespace Drivers.DAL.EF.Specifications;

/// <summary>
/// Базова абстрактна реалізація патерну Specification.
/// Надає зручні методи Fluent API (AddInclude, ApplyOrderBy, ApplyPaging) для формування запитів до БД.
/// </summary>
/// <typeparam name="T">Тип сутності</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    public Expression<Func<T, bool>>? Criteria { get; private set; }

    public List<Expression<Func<T, object?>>> Includes { get; } = new();

    public List<string> IncludeStrings { get; } = new();

    public Expression<Func<T, object>>? OrderBy { get; private set; }

    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    public int Take { get; private set; }

    public int Skip { get; private set; }

    public bool IsPagingEnabled { get; private set; }

    /// <summary>
    /// Конструктор без фільтрації (для вибірок усіх записів із зв'язками або сортуванням)
    /// </summary>
    protected BaseSpecification()
    {
    }

    /// <summary>
    /// Конструктор із зазначенням первинного предиката фільтрації
    /// </summary>
    /// <param name="criteria">Умова вибірки (WHERE)</param>
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <summary>
    /// Додавання навігаційної властивості для Eager Loading через сильно типізований Expression
    /// </summary>
    protected void AddInclude(Expression<Func<T, object?>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Додавання вкладеного зв'язку за рядковим шляхом (наприклад, "Expenses.Truck")
    /// </summary>
    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Застосування сортування за зростанням
    /// </summary>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Застосування сортування за спаданням
    /// </summary>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }

    /// <summary>
    /// Активація та налаштування пагінації (SKIP та TAKE)
    /// </summary>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}
