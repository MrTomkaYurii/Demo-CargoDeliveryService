var builder = DistributedApplication.CreateBuilder(args);

// 1. Інфраструктура (Full Aspire Orchestration): RabbitMQ з веб-інтерфейсом керування
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// 2. Інфраструктура: MS SQL Server та окремі бази даних для кожного мікросервісу
var sqlserver = builder.AddSqlServer("sqlserver");
var driversDb = sqlserver.AddDatabase("DriversManagement", "dev_driversmanagement");
var blogDb = sqlserver.AddDatabase("DriversBlogManagement", "dev_driversblogmanagement");
var cargoDb = sqlserver.AddDatabase("CargoOrderingService", "dev_cargoorderingservice");

// 3. Мікросервіс блогу водіїв (gRPC сервер + MassTransit консьюмер)
var blogApi = builder.AddProject<Projects.DriversBlogManagement>("driversblogmanagement")
    .WithReference(blogDb)
    .WithReference(rabbitmq)
    .WaitFor(blogDb)
    .WaitFor(rabbitmq);

// 4. Основний сервіс керування водіями (gRPC клієнт до блогу + MassTransit паблішер)
var driversApi = builder.AddProject<Projects.DriversManagement>("driversmanagement")
    .WithReference(driversDb)
    .WithReference(rabbitmq)
    .WithReference(blogApi)
    .WaitFor(driversDb)
    .WaitFor(rabbitmq)
    .WaitFor(blogApi);

// 5. Сервіс замовлень вантажів (MassTransit паблішер і консьюмер)
var cargoApi = builder.AddProject<Projects.CargoOrderingService>("cargoorderingservice")
    .WithReference(cargoDb)
    .WithReference(rabbitmq)
    .WaitFor(cargoDb)
    .WaitFor(rabbitmq);

builder.Build().Run();
