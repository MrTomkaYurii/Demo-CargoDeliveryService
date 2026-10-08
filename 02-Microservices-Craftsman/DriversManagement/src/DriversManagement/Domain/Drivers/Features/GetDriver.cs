namespace DriversManagement.Domain.Drivers.Features;

using DriversManagement.Domain.Drivers.Dtos;
using DriversManagement.Domain.Drivers.Services;
using DriversManagement.Exceptions;
using DriversManagement.Domain;
using HeimGuard;
using Mappings;
using MediatR;
using DriversBlogManagement;
using Grpc.Core;
using System.Linq;

/// <summary>
/// ====================================================================================
/// ⚡ CQRS FEATURE: GET DRIVER WITH GRPC BLOG POSTS (СИНХРОННИЙ gRPC КЛІЄНТ)
/// ------------------------------------------------------------------------------------
/// АРХІТЕКТУРНА КОНЦЕПЦІЯ:
///
/// 1. ПРИЗНАЧЕННЯ:
///    Цей CQRS-обробник зчитує картку водія з локальної бази даних DriversManagement DB
///    та робить СИНХРОННИЙ БІНАРНИЙ виклик по протоколу HTTP/2 через gRPC до віддаленого
///    мікросервісу DriversBlogManagement для отримання його публікацій.
///
/// 2. ЧОМУ ТУТ ВИКОРИСТОВУЄТЬСЯ gRPC ЗАМІСТЬ ЗВИЧАЙНОГО REST HTTP?
///    - Висока продуктивність: gRPC використовує постійний HTTP/2 мультиплексований канал,
///      що усуває затримки на відкриття нових TCP-з'єднань.
///    - Компактність: дані серіалізуються у двійковий формат Protocol Buffers (Google.Protobuf),
///      що значно економить пропускну здатність внутрішньої мережі.
///    - Суворий контракт: клас DriverRpc.DriverRpcClient компілюється безпосередньо з файлу
///      Driver.proto, гарантуючи 100% відповідність типів даних між клієнтом і сервером.
///
/// 3. ПАТЕРН СТІЙКОСТІ "GRACEFUL DEGRADATION" (ЕЛЕГАНТНА ДЕГРАДАЦІЯ):
///    Зверніть увагу на блок catch (RpcException):
///    Якщо сервіс блогів упав, перезавантажується або виник мережевий таймаут —
///    ми НЕ викидаємо помилку 500 для користувача! Ми повертаємо валідний профіль водія
///    із порожнім списком постів result.Posts = new List&lt;...&gt;().
///    Це ключовий принцип надійності мікросервісів — збій некритичного функціоналу
///    не повинен руйнувати базовий бізнес-процес.
///
/// 4. SERVICE DISCOVERY В .NET ASPIRE:
///    Клієнт зареєстрований у WebAppServiceConfiguration.cs через AddGrpcClient(...).
///    Адресація автоматично транслюється на запущений контейнер або процес
///    через ім'я "driversblogmanagement".
/// ====================================================================================
/// </summary>
public static class GetDriver
{
    /// <summary>
    /// Незмінний CQRS-запит (Query) на отримання водія за ID разом із постами.
    /// </summary>
    public sealed record Query(Guid DriverId) : IRequest<FullDataAboutDriverAndPostsDto>;

    /// <summary>
    /// Обробник запиту MediatR з інжекцією репозиторію та згенерованого gRPC-клієнта.
    /// </summary>
    public sealed class Handler : IRequestHandler<Query, FullDataAboutDriverAndPostsDto>
    {
        private readonly IDriverRepository _driverRepository;
        private readonly IHeimGuardClient _heimGuard;
        private readonly DriverRpc.DriverRpcClient _client;

        public Handler(
            IDriverRepository driverRepository, 
            IHeimGuardClient heimGuard,
            DriverRpc.DriverRpcClient client)
        {
            _driverRepository = driverRepository;
            _heimGuard = heimGuard;
            _client = client;
        }

        public async Task<FullDataAboutDriverAndPostsDto> Handle(Query request, CancellationToken cancellationToken)
        {
            // Перевірка дозволів безпеки за допомогою політики доступу HeimGuard
            await _heimGuard.MustHavePermission<ForbiddenAccessException>(Permissions.CanReadDriver);

            var result = new FullDataAboutDriverAndPostsDto();

            // КРОК 1: Зчитуємо сутність водія з локальної бази даних DriversManagement DB
            Driver drv = await _driverRepository.GetById(request.DriverId, cancellationToken: cancellationToken);
            DriverDto drvDTO = drv.ToDriverDto();

            result.Driver = drvDTO;

            // КРОК 2: Синхронний віддалений RPC-виклик до мікросервісу DriversBlogManagement
            try
            {
                // Створюємо Protobuf-запит згідно з контрактом Driver.proto
                var grpcRequest = new DriverWithPostsRequest { DriverId = request.DriverId.ToString() };

                // Виконуємо бінарний HTTP/2 виклик з прокиданням CancellationToken
                DriverWithPostsResponse grpcResponse = await _client.GetDriverWithPostsAsync(
                    grpcRequest, 
                    cancellationToken: cancellationToken);

                // Мапимо отримані Protobuf-повідомлення в DTO домену
                if (grpcResponse?.Posts != null)
                {
                    result.Posts = grpcResponse.Posts.Select(p => new DriversBlogManagement.Domain.PostAboutDrivers.Dtos.PostAboutDriverDto
                    {
                        Id = Guid.TryParse(p.Id, out var postId) ? postId : Guid.Empty,
                        Title = p.Title,
                        Content = p.Content
                    }).ToList();
                }
            }
            catch (RpcException)
            {
                // Graceful Degradation: якщо віддалений сервіс блогів недоступний,
                // ми повертаємо дані водія з порожньою колекцією постів, рятуючи запит користувача
                result.Posts = new List<DriversBlogManagement.Domain.PostAboutDrivers.Dtos.PostAboutDriverDto>();
            }

            return result;
        }
    }
}
