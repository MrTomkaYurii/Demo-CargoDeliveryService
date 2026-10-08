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
// Aspire автоматично завантажує Docker-образ RabbitMQ та налаштовує порти:
// - AMQP порт 5672 для передачі бінарних фреймів повідомлень.
// - HTTP порт 15672 для веб-панелі адміністрування (WithManagementPlugin).
// Змінна ConnectionStrings__rabbitmq автоматично інжектується в залежні сервіси.
// ====================================================================================
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// ====================================================================================
// 💾 2. ІНФРАСТРУКТУРА: РЕЛЯЦІЙНА БАЗА ДАНИХ MS SQL SERVER 2022
// ------------------------------------------------------------------------------------
// Патерн "Database-per-Microservice": кожен Bounded Context має власну фізичну або
// логічну БД, унеможливлюючи несанкціонований доступ до чужих таблиць напряму через SQL.
// ====================================================================================
var sqlserver = builder.AddSqlServer("sqlserver");
var driversDb = sqlserver.AddDatabase("DriversManagement", "dev_driversmanagement");
var blogDb = sqlserver.AddDatabase("DriversBlogManagement", "dev_driversblogmanagement");
var cargoDb = sqlserver.AddDatabase("CargoOrderingService", "dev_cargoorderingservice");

// ====================================================================================
// 📦 3. МІКРОСЕРВІС ЗАМОВЛЕНЬ ТА ДОСТАВОК: CargoOrderingService
// ------------------------------------------------------------------------------------
// - Володіє логістичним доменом (Order, Delivery, Carrier).
// - MassTransit Consumer для IDriverCreated / IDriverUpdated (актуалізація водіїв).
// - MassTransit Publisher для IOrderCreated.
// - Надає HTTP REST ендпоінти для отримання активних рейсів водія.
// ====================================================================================
var cargoApi = builder.AddProject<Projects.CargoOrderingService>("cargoorderingservice")
    .WithReference(cargoDb)
    .WithReference(rabbitmq)
    .WaitFor(cargoDb)
    .WaitFor(rabbitmq);

// ====================================================================================
// 📝 4. МІКРОСЕРВІС БЛОГУ ВОДІЇВ: DriversBlogManagement
// ------------------------------------------------------------------------------------
// - Соціальний домен (Post, Comment, Like, Shadow Driver).
// - gRPC Server: реалізує DriverRpcService (HTTP/2 бінарний контракт).
// - MassTransit Consumer: асинхронно синхронізує Shadow Drivers.
// - HTTP REST API: надає пости водіїв для зовнішніх клієнтів та BFF Агрегатора.
// ====================================================================================
var blogApi = builder.AddProject<Projects.DriversBlogManagement>("driversblogmanagement")
    .WithReference(blogDb)
    .WithReference(rabbitmq)
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
    .WithReference(driversDb)
    .WithReference(rabbitmq)
    .WithReference(blogApi)
    .WithReference(cargoApi)
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
// - .WaitFor(...) гарантує, що агрегатор стартує лише після готовності залежних API.
// ====================================================================================
var httpAggregator = builder.AddProject<Projects.CargoDelivery_HttpAggregator>("httpaggregator")
    .WithReference(driversApi)
    .WithReference(cargoApi)
    .WithReference(blogApi)
    .WaitFor(driversApi)
    .WaitFor(cargoApi)
    .WaitFor(blogApi);

builder.Build().Run();
