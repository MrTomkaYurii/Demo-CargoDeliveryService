using Microsoft.EntityFrameworkCore;

namespace Drivers.DAL.EF.Helpers;

/// <summary>
/// ПАТЕРН ПАГІНАЦІЇ (Pagination Pattern):
/// Спеціалізована колекція-обгортка, яка інкапсулює результати вибірки поточної сторінки 
/// та супутні метадані (загальна кількість, поточна сторінка, розмір сторінки, наявність наступної/попередньої).
/// 
/// 💡 Архітектурна перевага:
/// 1. Захист від OutOfMemory: Запити в БД обмежуються через SQL OFFSET / FETCH, сервер не тягне всі рядки.
/// 2. Відокремлення обчислення сторінок від контролера (SRP).
/// 3. Підтримка CancellationToken для скасування важких запитів підрахунку та вибірки.
/// </summary>
/// <typeparam name="T">Тип доменної сутності або DTO</typeparam>
public class PagedList<T> : List<T>
{
    /// <summary>Поточний номер сторінки (1-based index)</summary>
    public int CurrentPage { get; private set; }

    /// <summary>Загальна кількість сторінок, обчислена за формулою: Ceil(TotalCount / PageSize)</summary>
    public int TotalPages { get; private set; }

    /// <summary>Кількість записів на одній сторінці</summary>
    public int PageSize { get; private set; }

    /// <summary>Загальна кількість записів у базі даних (до застосування Skip/Take)</summary>
    public int TotalCount { get; private set; }

    /// <summary>Чи є попередня сторінка (корисно для UI кнопок "Попередня")</summary>
    public bool HasPrevious => CurrentPage > 1;

    /// <summary>Чи є наступна сторінка (корисно для UI кнопок "Наступна")</summary>
    public bool HasNext => CurrentPage < TotalPages;

    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        // Обчислення загальної кількості сторінок з округленням вгору
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);

        AddRange(items);
    }

    /// <summary>
    /// Фабричний метод для пагінації IQueryable-вибірки (відкладене синхронне виконання в БД).
    /// </summary>
    public static PagedList<T> ToPagedList(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }

    /// <summary>
    /// Асинхронний фабричний метод для пагінації з обов'язковою підтримкою CancellationToken.
    /// Перериває запити COUNT(*) та OFFSET/FETCH на рівні SQL Server при скасуванні токена.
    /// </summary>
    public static async Task<PagedList<T>> ToPagedListAsync(
        IQueryable<T> source, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken);
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
