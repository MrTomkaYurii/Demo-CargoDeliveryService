namespace DriversBlogManagement.Controllers.v1;

using DriversBlogManagement.Databases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// ====================================================================================
/// 🔒 ВНУТРІШНІЙ (SERVICE-TO-SERVICE) API ПОСТІВ ПРО ВОДІЇВ
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Read-only ендпоінт для синхронних HTTP-викликів від агрегаторів
/// (CargoDelivery.HttpAggregator та BFF у DriversManagement): повертає останні пости
/// конкретного водія в компактному вигляді ("дорожні замітки").
///
/// ЧОМУ НЕ ЗВИЧАЙНИЙ /api/postaboutdrivers?
/// - Публічний контролер захищений [Authorize] + HeimGuard (потрібен JWT користувача).
/// - Публічний список не фільтрує пости за водієм.
///
/// БЕЗПЕКА:
/// [AllowAnonymous] — спрощення навчального стенду. У продакшні внутрішні API закривають
/// мережевими політиками, mTLS або OAuth2 Client Credentials (токен самого сервісу).
/// ====================================================================================
/// </summary>
[ApiController]
[Route("api/internal/postaboutdrivers")]
[ApiVersion("1.0")]
[AllowAnonymous]
public sealed class InternalPostAboutDriversController : ControllerBase
{
    // Скільки символів контенту віддаємо як короткий анонс (Summary)
    private const int SummaryLength = 200;

    private readonly BlogManagementContext _db;

    public InternalPostAboutDriversController(BlogManagementContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Повертає до 10 останніх постів водія. Приклад: GET api/internal/postaboutdrivers?driverId={guid}
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByDriver([FromQuery] Guid driverId, CancellationToken cancellationToken)
    {
        var posts = await _db.PostAboutDrivers
            .AsNoTracking()
            .Where(p => p.Driver != null && p.Driver.Id == driverId)
            .OrderByDescending(p => p.CreatedOn)
            .Take(10)
            .Select(p => new { p.Id, p.Title, p.Content, p.CreatedOn })
            .ToListAsync(cancellationToken);

        // Обрізання тексту робимо вже в пам'яті (після SQL), щоб не ускладнювати запит
        var result = posts.Select(p => new
        {
            PostId = p.Id,
            p.Title,
            Summary = p.Content is { Length: > SummaryLength } ? p.Content[..SummaryLength] + "…" : p.Content ?? string.Empty,
            // У BaseEntity блогу CreatedOn має тип DateTime (на відміну від DateTimeOffset у CargoOrderingService)
            CreatedAtUtc = p.CreatedOn
        });

        return Ok(result);
    }

    /// <summary>
    /// 🧪 ДЕМО-СІД: створює "дорожню замітку" для тіньового водія, щоб агрегатор мав що показати.
    /// Приклад: POST api/internal/postaboutdrivers/demo-seed?driverId={guid}
    /// <para>
    /// Публічне API блогу не вміє прив'язати пост до водія, а в моделі Craftsman зв'язок
    /// Driver ↔ PostAboutDriver — "один-до-одного". Тому створюємо (не більше одного) пост і
    /// виставляємо тіньовий зовнішній ключ DriverId напряму через EF Core ChangeTracker.
    /// </para>
    /// </summary>
    [HttpPost("demo-seed")]
    public async Task<IActionResult> SeedDemoPost([FromQuery] Guid driverId, CancellationToken cancellationToken)
    {
        // Водій має вже існувати в блозі — тобто подія IDriverCreated мала дійти через RabbitMQ
        var driverExists = await _db.Drivers.AnyAsync(d => d.Id == driverId, cancellationToken);
        if (!driverExists)
            return NotFound(new { message = $"Тіньового водія {driverId} ще немає в блозі (подія IDriverCreated не оброблена?)." });

        if (await _db.PostAboutDrivers.AnyAsync(p => p.Driver.Id == driverId, cancellationToken))
            return Conflict(new { message = "Для цього водія демо-пост уже існує (зв'язок один-до-одного)." });

        var post = Domain.PostAboutDrivers.PostAboutDriver.Create(new Domain.PostAboutDrivers.Models.PostAboutDriverForCreation
        {
            Title = "Траса М-06: ремонт мосту",
            Content = "Біля Житомира звужено рух до однієї смуги, затримка 15–20 хвилин. Об'їзд через Коростишів."
        });
        await _db.PostAboutDrivers.AddAsync(post, cancellationToken);

        // Тіньова властивість (shadow property) "DriverId" не має C#-властивості в сутності —
        // EF Core дозволяє задати її значення через Entry(...).Property(...)
        _db.Entry(post).Property("DriverId").CurrentValue = driverId;
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { postId = post.Id, driverId });
    }
}
