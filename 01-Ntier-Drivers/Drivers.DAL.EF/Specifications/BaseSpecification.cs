using Drivers.DAL.EF.Specifications.Contracts;
using System.Linq.Expressions;

namespace Drivers.DAL.EF.Specifications;

/// <summary>
/// Базова абстрактна реалізація патерну Specification.
/// <para>
/// Надає зручні методи Fluent API (<see cref="AddInclude(Expression{Func{T, object?}})"/>, 
/// <see cref="ApplyOrderBy(Expression{Func{T, object}})"/>, <see cref="ApplyPaging(int, int)"/>) 
/// для декларативного конструювання бізнес-запитів до бази даних у класах-нащадках.
/// </para>
/// </summary>
/// <typeparam name="T">Тип сутності бази даних</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    /// <inheritdoc/>
    public Expression<Func<T, bool>>? Criteria { get; private set; }

    /// <inheritdoc/>
    public List<Expression<Func<T, object?>>> Includes { get; } = new();

    /// <inheritdoc/>
    public List<string> IncludeStrings { get; } = new();

    /// <inheritdoc/>
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    /// <inheritdoc/>
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    /// <inheritdoc/>
    public int Take { get; private set; }

    /// <inheritdoc/>
    public int Skip { get; private set; }

    /// <inheritdoc/>
    public bool IsPagingEnabled { get; private set; }

    /// <summary>
    /// Конструктор за замовчуванням (без початкового критерію фільтрації).
    /// Використовується для специфікацій вибірки всіх сутностей з Eager Loading або специфічним сортуванням.
    /// </summary>
    protected BaseSpecification()
    {
    }

    /// <summary>
    /// Конструктор із первинним предикатом фільтрації.
    /// Приймає дерево виразів <paramref name="criteria"/>, яке буде трансльовано в T-SQL <c>WHERE</c>.
    /// </summary>
    /// <param name="criteria">Дерево синтаксису (AST) для умови вибірки</param>
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criteria = criteria;
    }

    /// <summary>
    /// Додавання навігаційної властивості для жадібного завантаження (Eager Loading) через типізоване дерево виразів.
    /// <para>
    /// <i>Приклад:</i> <c>AddInclude(d => d.Company);</c>
    /// </para>
    /// </summary>
    /// <param name="includeExpression">Вираз звернення до навігаційної властивості сутності</param>
    protected void AddInclude(Expression<Func<T, object?>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    /// <summary>
    /// Додавання вкладеного зв'язку за рядковим шляхом (для багаторівневих залежностей через ThenInclude).
    /// <para>
    /// <i>Приклад:</i> <c>AddInclude("Expenses.Truck");</c>
    /// </para>
    /// </summary>
    /// <param name="includeString">Рядковий шлях навігаційного зв'язку</param>
    protected void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    /// <summary>
    /// Налаштування сортування за зростанням (ORDER BY ... ASC).
    /// </summary>
    /// <param name="orderByExpression">Вираз властивості, за якою виконується сортування</param>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    /// <summary>
    /// Налаштування сортування за спаданням (ORDER BY ... DESC).
    /// </summary>
    /// <param name="orderByDescExpression">Вираз властивості, за якою виконується сортування</param>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
    {
        OrderByDescending = orderByDescExpression;
    }

    /// <summary>
    /// Активація та налаштування параметрів серверної пагінації.
    /// Транслюється в T-SQL: <c>OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY</c>.
    /// </summary>
    /// <param name="skip">Кількість рядків для пропуску (Offset)</param>
    /// <param name="take">Розмір сторінки (PageSize / Limit)</param>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }
}
