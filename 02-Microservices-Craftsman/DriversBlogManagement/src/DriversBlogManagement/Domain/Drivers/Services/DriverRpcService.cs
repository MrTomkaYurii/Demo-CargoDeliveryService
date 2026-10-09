namespace DriversBlogManagement.Domain.Drivers.Services;

using DriversBlogManagement;
using Grpc.Core;
using System.Threading.Tasks;

/// <summary>
/// ====================================================================================
/// ⚡ АРХІТЕКТУРНИЙ ПАТЕРН: СИНХРОННИЙ gRPC СЕРВЕР (SERVICE-TO-SERVICE RPC)
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Цей сервіс реалізує серверний ендпоінт gRPC для надання постів водія за запитом
/// від мікросервісу DriversManagement.
///
/// ЧОМУ САМЕ gRPC ДЛЯ ВНУТРІШНЬОГО ЗВ'ЯЗКУ МІЖ СЕРВІСАМИ?
/// 1. Протокол HTTP/2:
///    На відміну від HTTP/1.1 REST, де на кожен запит відкривається окремий TCP-handshake,
///    gRPC підтримує мультиплексування (Multiplexing) — сотні запитів передаються паралельно
///    через одне відкрите постійне TCP-з'єднання.
/// 2. Бінарний формат Protocol Buffers (Protobuf):
///    Дані пакуються в компактні байти замість важкого тексту JSON. Це в 5-10 разів зменшує
///    обсяг трафіку і не витрачає час процесора на парсинг строк.
/// 3. Суворий контракт (Driver.proto):
///    Базовий клас DriverRpc.DriverRpcBase та моделі DriverWithPostsRequest/Response
///    генеруються автоматично компілятором protoc на етапі збірки проєкту (MSBuild).
/// ====================================================================================
/// </summary>
public class DriverRpcService : DriverRpc.DriverRpcBase
{
    // Залежимо від АБСТРАКЦІЇ (IDriverRepository), а не від конкретного класу: DI-контейнер
    // реєструє репозиторії саме за інтерфейсами (AddBoundaryServices у WebAppServiceConfiguration).
    // Раніше тут був конкретний DriverRepository → DI не міг створити сервіс, і КОЖЕН gRPC-виклик
    // завершувався статусом Unknown (2), який клієнт тихо маскував через Graceful Degradation.
    private readonly IDriverRepository _driverRepository;

    /// <summary>
    /// Конструктор інжектує локальний репозиторій блогу для читання постів водія.
    /// </summary>
    public DriverRpcService(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    /// <summary>
    /// ====================================================================================
    /// МЕТОД: GetDriverWithPosts
    /// ------------------------------------------------------------------------------------
    /// Вхідний Protobuf-запит містить driver_id.
    /// Сервер зчитує водія та його публікації з локальної бази DriversBlog DB
    /// і повертає строго типізовану бінарну Protobuf-відповідь.
    ///
    /// ServerCallContext context:
    /// Дозволяє перевіряти скасування запиту клієнтом (context.CancellationToken),
    /// зчитувати метадані (заголовки авторизації) та передавати трейсинг OpenTelemetry.
    /// ====================================================================================
    /// </summary>
    public override async Task<DriverWithPostsResponse> GetDriverWithPosts(
        DriverWithPostsRequest request, 
        ServerCallContext context)
    {
        try
        {
            // КРОК 1: Парсимо GUID водія з бінарного контракту
            if (!Guid.TryParse(request.DriverId, out var driverId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Некоректний формат DriverId GUID"));
            }

            // КРОК 2: Зчитуємо дані з локального контексту блогу (EF Core)
            var driverWithPostsDto = await _driverRepository.GetDriverWithPosts(driverId);
            if (driverWithPostsDto == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Водія {driverId} не знайдено у блозі"));
            }

            // КРОК 3: Формуємо типізовану бінарну відповідь Protobuf
            var response = new DriverWithPostsResponse
            {
                DriverId = driverWithPostsDto.DriverId.ToString(),
                FirstName = driverWithPostsDto.FirstName ?? string.Empty,
                LastName = driverWithPostsDto.LastName ?? string.Empty
            };

            // Додаємо пости до повторюваного поля (repeated PostAboutDriverResponse posts)
            foreach (var post in driverWithPostsDto.Posts)
            {
                response.Posts.Add(new PostAboutDriverResponse
                {
                    Id = post.Id.ToString(),
                    Title = post.Title ?? string.Empty,
                    Content = post.Content ?? string.Empty
                });
            }

            return response;
        }
        catch (RpcException)
        {
            // Прокидаємо відомі gRPC винятки без змін
            throw;
        }
        catch (Exception ex)
        {
            // Перетворюємо будь-яку непередбачувану системну помилку у стандартизований gRPC статус
            throw new RpcException(new Status(StatusCode.Internal, $"Внутрішня помилка gRPC сервера: {ex.Message}"));
        }
    }
}
