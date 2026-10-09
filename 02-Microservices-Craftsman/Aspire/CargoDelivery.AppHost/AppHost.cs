// ====================================================================================
// 🚀 .NET 10 ASPIRE APPHOST - ГОЛОВНИЙ ДИРИГЕНТ ОРКЕСТРАЦІЇ РОЗПОДІЛЕНОЇ СИСТЕМИ
// ------------------------------------------------------------------------------------
// Замість складного docker-compose.yaml та розрізнених скриптів, AppHost описує всю
// топологію проєкту чистою, типізованою мовою C#:
// 1. Контейнери інфраструктури (RabbitMQ Broker, MS SQL Server).
// 2. Бази даних для кожного ізольованого Bounded Context.
// 3. Мікросервісні Web API (.NET 10).
// 4. Декларативні залежності та усунення Race Conditions через .WaitFor().
// 5. Автоматичне впровадження Service Discovery та Connection Strings.
// ====================================================================================

var builder = DistributedApplication.CreateBuilder(args);

// ====================================================================================
// 🐰 1. ІНФРАСТРУКТУРА: BROKER RABBITMQ (AMQP 0-9-1)
// ------------------------------------------------------------------------------------
// Aspire автоматично завантажує Docker-образ RabbitMQ (rabbitmq:4.x-management) і публікує:
// - AMQP-ендпоінт (всередині контейнера 5672) — для бінарних фреймів повідомлень MassTransit;
// - HTTP-ендпоінт (всередині контейнера 15672) — веб-панель адміністрування (WithManagementPlugin).
// Зовнішні порти на хості призначаються ДИНАМІЧНО — дивіться колонку URLs у дашборді.
// Логін за замовчуванням — guest, а пароль Aspire ГЕНЕРУЄ і зберігає як параметр
// "rabbitmq-password" (вкладка Parameters у дашборді, user secrets AppHost).
// Залежні сервіси отримують готовий рядок ConnectionStrings__rabbitmq
// (amqp://guest:{пароль}@localhost:{порт}) через .WithReference(rabbitmq).
// ====================================================================================
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// ====================================================================================
// 💾 2. ІНФРАСТРУКТУРА: РЕЛЯЦІЙНА БАЗА ДАНИХ MS SQL SERVER 2022
// ------------------------------------------------------------------------------------
// Патерн "Database-per-Microservice": кожен Bounded Context має власну фізичну або
// логічну БД, унеможливлюючи несанкціонований доступ до чужих таблиць напряму через SQL.
//
// ⚠️ ВАЖЛИВО: імена ресурсів в Aspire РЕГІСТРОНЕЗАЛЕЖНІ та мають бути унікальними.
// Якщо назвати базу "CargoOrderingService", а проєкт — "cargoorderingservice", AppHost
// впаде на старті з DistributedApplicationException ("resource ... already exists").
// Тому ресурси БД мають суфікс "-db", а очікуване сервісом ім'я рядка підключення
// (ConnectionStrings:CargoOrderingService тощо) задається явно через connectionName
// у .WithReference(...) нижче.
//
// Другий аргумент AddDatabase — фізична назва БД всередині SQL Server. Якщо БД ще
// не існує, Aspire створює її сам; таблиці потім створюють EF Core міграції
// (MigrationHostedService у кожному мікросервісі).
// ====================================================================================
var sqlserver = builder.AddSqlServer("sqlserver");
var driversDb = sqlserver.AddDatabase("driversmanagement-db", "dev_driversmanagement");
var blogDb = sqlserver.AddDatabase("driversblogmanagement-db", "dev_driversblogmanagement");
var cargoDb = sqlserver.AddDatabase("cargoorderingservice-db", "dev_cargoorderingservice");

// ====================================================================================
// 📦 3. МІКРОСЕРВІС ЗАМОВЛЕНЬ ТА ДОСТАВОК: CargoOrderingService
// ------------------------------------------------------------------------------------
// - Володіє логістичним доменом (Order, Delivery, Carrier).
// - MassTransit Consumer для IDriverCreated / IDriverUpdated (актуалізація водіїв).
// - MassTransit Publisher для IOrderCreated.
// - Внутрішній REST-ендпоінт api/internal/deliveries для агрегаторів.
// ====================================================================================
var cargoApi = builder.AddProject<Projects.CargoOrderingService>("cargoorderingservice")
    // connectionName → змінна ConnectionStrings__CargoOrderingService, яку читає ConnectionStringOptions сервісу
    .WithReference(cargoDb, connectionName: "CargoOrderingService")
    .WithReference(rabbitmq)
    // Health probe для Aspire: ресурс вважається "Healthy" лише коли GET /health повертає 200.
    // Саме на цей стан чекають залежні ресурси у .WaitFor(cargoApi).
    .WithHttpHealthCheck("/health")
    .WaitFor(cargoDb)
    .WaitFor(rabbitmq);

// ====================================================================================
// 📝 4. МІКРОСЕРВІС БЛОГУ ВОДІЇВ: DriversBlogManagement
// ------------------------------------------------------------------------------------
// - Соціальний домен (Post, Comment, Like, Shadow Driver).
// - gRPC Server: реалізує DriverRpcService (HTTP/2 бінарний контракт).
// - MassTransit Consumer: асинхронно синхронізує Shadow Drivers.
// - HTTP REST API: публічні ендпоінти блогу + внутрішній api/internal/postaboutdrivers для агрегаторів.
// ====================================================================================
var blogApi = builder.AddProject<Projects.DriversBlogManagement>("driversblogmanagement")
    .WithReference(blogDb, connectionName: "DriversBlogManagement")
    .WithReference(rabbitmq)
    .WithHttpHealthCheck("/health")
    .WaitFor(blogDb)
    .WaitFor(rabbitmq);

// ====================================================================================
// 🚚 5. ГОЛОВНИЙ СЕРВІС ВОДІЇВ ТА BFF АГРЕГАТОР: DriversManagement
// ------------------------------------------------------------------------------------
// - Володіє повним досьє водія (Driver Aggregate Root).
// - MassTransit Publisher: ініціює події створення та оновлення водія.
// - gRPC Client: викликає DriversBlogManagement напряму по HTTP/2 Protobuf.
// - HTTP Gateway Aggregator / BFF: паралельно опитує CargoOrderingService та
//   DriversBlogManagement через Task.WhenAll для формування зведеного дашборду.
// - .WaitFor(...) гарантує, що сервіс стартує лише після повної готовності БД, брокера
//   та суміжних API, усуваючи неприємні стартові винятки Connection Refused!
// ====================================================================================
var driversApi = builder.AddProject<Projects.DriversManagement>("driversmanagement")
    .WithReference(driversDb, connectionName: "DriversManagement")
    .WithReference(rabbitmq)
    .WithReference(blogApi)
    .WithReference(cargoApi)
    .WithHttpHealthCheck("/health")
    .WaitFor(driversDb)
    .WaitFor(rabbitmq)
    .WaitFor(blogApi)
    .WaitFor(cargoApi);

// ====================================================================================
// 🛡️ 6. GATEWAY AGGREGATOR ДИСПЕТЧЕРА: CargoDelivery.HttpAggregator
// ------------------------------------------------------------------------------------
// - Патерн Scatter-Gather: паралельний Fan-Out / Fan-In через IHttpClientFactory.
// - Демонстрація OpenTelemetry Distributed Tracing (батьківські та дочірні спани).
// - Демонстрація Polly Resilience (Retry, Timeout, Circuit Breaker).
// - .WaitFor(...) гарантує, що агрегатор стартує лише після того, як залежні API стануть
//   Healthy (їхній /health повертає 200 — див. .WithHttpHealthCheck вище).
// ====================================================================================
var httpAggregator = builder.AddProject<Projects.CargoDelivery_HttpAggregator>("httpaggregator")
    .WithReference(driversApi)
    .WithReference(cargoApi)
    .WithReference(blogApi)
    .WithHttpHealthCheck("/health")
    .WaitFor(driversApi)
    .WaitFor(cargoApi)
    .WaitFor(blogApi);

builder.Build().Run();
