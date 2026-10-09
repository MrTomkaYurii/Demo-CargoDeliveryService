namespace CargoOrderingService.Controllers.v1;

using CargoOrderingService.Databases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// ====================================================================================
/// 🔒 ВНУТРІШНІЙ (SERVICE-TO-SERVICE) API ДОСТАВОК
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Легкий read-only ендпоінт для синхронних викликів від інших мікросервісів
/// (CargoDelivery.HttpAggregator та BFF у DriversManagement). Повертає ЛИШЕ доставки
/// конкретного водія у плоскому вигляді, потрібному агрегаторам.
///
/// ЧОМУ НЕ ЗВИЧАЙНИЙ /api/deliveries?
/// 1. Публічний DeliveriesController захищений [Authorize] (JWT від Keycloak) та
///    перевірками дозволів HeimGuard — сервіс-клієнт без токена отримав би 401.
/// 2. Публічний ендпоінт повертає ВСІ доставки з пагінацією, без фільтра за водієм.
///
/// БЕЗПЕКА (важливо розуміти!):
/// [AllowAnonymous] тут — свідоме спрощення для навчального локального стенду Aspire.
/// У продакшні внутрішній API захищають інакше, ніж зовнішній:
///   - мережеві політики (ендпоінт недоступний з Ingress/інтернету);
///   - mTLS між сервісами (service mesh: Istio, Linkerd);
///   - OAuth2 Client Credentials — сервіс отримує власний токен зі scope "deliveries.read".
/// ====================================================================================
/// </summary>
[ApiController]
[Route("api/internal/deliveries")]
[ApiVersion("1.0")]
[AllowAnonymous]
public sealed class InternalDeliveriesController : ControllerBase
{
    private readonly OrderingContext _db;

    public InternalDeliveriesController(OrderingContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Повертає доставки водія. Приклад: GET api/internal/deliveries?driverId={guid}
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetByDriver([FromQuery] Guid driverId, CancellationToken cancellationToken)
    {
        // AsNoTracking: дані лише читаються, тож EF Core не витрачає пам'ять на трекінг змін.
        // Фільтр Driver.Id працює по тіньовій (shadow) копії водія, яку сервіс отримав
        // асинхронно через подію IDriverCreated (MassTransit + RabbitMQ).
        var deliveries = await _db.Deliveries
            .AsNoTracking()
            .Where(d => d.Driver != null && d.Driver.Id == driverId)
            .OrderByDescending(d => d.CreatedOn)
            .Select(d => new
            {
                OrderId = d.Id,
                CargoDescription = d.PackageDetails,
                PickupAddress = d.PickupAddress,
                DestinationAddress = d.DestinationAddress,
                DeliveryDate = d.DeliveryDate,
                Status = d.DeliveryStatus
            })
            .ToListAsync(cancellationToken);

        return Ok(deliveries);
    }

    /// <summary>
    /// 🧪 ДЕМО-СІД: створює доставку, призначену тіньовому водієві, щоб агрегатор мав що показати.
    /// Приклад: POST api/internal/deliveries/demo-seed?driverId={guid}
    /// <para>
    /// Публічне API замовлень не має операції "призначити водія", хоча доменна модель це вміє
    /// (Delivery.SetDriver). Цей ендпоінт використовує саме доменний метод — без обходу інваріантів.
    /// </para>
    /// </summary>
    [HttpPost("demo-seed")]
    public async Task<IActionResult> SeedDemoDelivery([FromQuery] Guid driverId, CancellationToken cancellationToken)
    {
        // Тіньовий водій з'являється тут асинхронно — через подію IDriverCreated (MassTransit + RabbitMQ)
        var driver = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == driverId, cancellationToken);
        if (driver is null)
            return NotFound(new { message = $"Тіньового водія {driverId} ще немає в CargoOrderingService (подія IDriverCreated не оброблена?)." });

        var delivery = Domain.Deliveries.Delivery.Create(new Domain.Deliveries.Models.DeliveryForCreation
        {
            DeliveryDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
            PickupAddress = "Київ, вул. Промислова, 12",
            DestinationAddress = "Львів, вул. Городоцька, 15",
            PackageDetails = "Будівельні матеріали, 18 палет",
            DeliveryStatus = "InTransit"
        }).SetDriver(driver);

        await _db.Deliveries.AddAsync(delivery, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Ok(new { orderId = delivery.Id, driverId });
    }
}
