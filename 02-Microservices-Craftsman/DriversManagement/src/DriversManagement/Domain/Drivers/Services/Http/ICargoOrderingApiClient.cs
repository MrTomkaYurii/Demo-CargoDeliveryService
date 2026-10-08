namespace DriversManagement.Domain.Drivers.Services.Http;

using DriversManagement.Domain.Drivers.Dtos;

/// <summary>
/// ====================================================================================
/// 🌐 АРХІТЕКТУРНИЙ ПАТЕРН: TYPED HTTP CLIENT (ТИПІЗОВАНИЙ HTTP-КЛІЄНТ)
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Інтерфейс клієнта для виконання синхронних HTTP REST-запитів до мікросервісу
/// замовлень вантажів (CargoOrderingService).
///
/// ЧОМУ ВИКОРИСТОВУЄТЬСЯ ІНТЕРФЕЙС ТА ТИПІЗОВАНИЙ КЛІЄНТ?
/// 1. Принцип інверсії залежностей (DIP з SOLID):
///    Наш CQRS Handler (GetDriverDashboard) залежить від абстракції (ICargoOrderingApiClient),
///    а не від конкретного мережевого коду чи класу HttpClient. Це дозволяє легко писати
///    Unit-тести, підставляючи Fake/Mock без реальних мережевих викликів.
/// 2. Інкапсуляція мережевих деталей:
///    Всі URL-шляхи (/api/v1/deliveries), параметри запиту та десеріалізація JSON
///    приховані всередині імплементації клієнта.
/// ====================================================================================
/// </summary>
public interface ICargoOrderingApiClient
{
    /// <summary>
    /// Отримує список активних доставок для вказаного водія за його ідентифікатором.
    /// </summary>
    /// <param name="driverId">Унікальний ідентифікатор водія (Guid).</param>
    /// <param name="cancellationToken">Токен скасування для переривання виклику при таймауті або відміні клієнтом.</param>
    /// <returns>Список доставок водія у форматі DTO.</returns>
    Task<List<AggregatedDeliveryDto>> GetActiveDeliveriesByDriverAsync(Guid driverId, CancellationToken cancellationToken = default);
}
