# 🚀 Архітектурний Майстер-Клас: .NET 10 Aspire Orchestration & Distributed Telemetry

> **Проєкт:** `Demo-CargoDeliveryService` • **Модуль:** `02-Microservices-Craftsman (Block 2)`  
> **Стек:** .NET 10 • .NET Aspire 13.6 • AppHost • ServiceDefaults • OpenTelemetry (OTLP) • RabbitMQ 4.x • MS SQL Server 2022 • gRPC • MassTransit 8
> **Як читати:** кожен слайд — 5–10 хвилин. Посилання на код ведуть до конкретних рядків (`файл#Lрядок`); у VS Code відкривайте прев'ю `Ctrl+Shift+V` і клікайте.

---

<a id="toc"></a>
## 📑 Зміст Майстер-Класу (Навігація по слайдах)

- [Слайд 1. Вступ і Маніфест .NET Aspire: Чому мікросервіси були болем до нього](#slide-1)
- [Слайд 2. Архітектурний ландшафт CargoDelivery: 3 API, gRPC, MassTransit та Інфраструктура](#slide-2)
- [Слайд 3. Проєкт CargoDelivery.AppHost — C# як диригент оркестру замість Docker Compose](#slide-3)
- [Слайд 4. Full Aspire Orchestration: Декларативний запуск RabbitMQ та SQL Server з коду](#slide-4)
- [Слайд 5. Патерн .WaitFor(): Подолання Race Conditions при старті розподілених систем](#slide-5)
- [Слайд 6. Проєкт CargoDelivery.ServiceDefaults: Універсальний "швейцарський ніж" мікросервісу](#slide-6)
- [Слайд 7. Прощавай, Jaeger! Перехід на нативний OTLP Pipeline у .NET Aspire Dashboard](#slide-7)
- [Слайд 8. Distributed Tracing для MassTransit + RabbitMQ: Сквозний трейсинг EDA-подій](#slide-8)
- [Слайд 9. Service Discovery на практиці: Як DriversManagement знаходить gRPC у DriversBlogManagement](#slide-9)
- [Слайд 10. Інтелектуальний зв'язок з RabbitMQ: Гібридний fallback (Aspire vs appsettings vs In-Memory)](#slide-10)
- [Слайд 11. Health Checks та Probes: Анатомія ендпоінтів /health та /alive](#slide-11)
- [Слайд 12. Resilience & Polly у ServiceDefaults: Стандартна стійкість HTTP-клієнтів](#slide-12)
- [Слайд 13. Aspire Dashboard: Детальний гайд по інтерфейсу телеметрії (Logs, Traces, Metrics)](#slide-13)
- [Слайд 14. Міграція всієї підсистеми на .NET 10 та стабілізація стенду: 149/149 зелених тестів](#slide-14)
- [Слайд 15. Чек-лист та практична інструкція запуску для новачка ("Як для чайників")](#slide-15)

---

<a id="slide-1"></a>
### 🎬 Слайд 1. Вступ і Маніфест .NET Aspire: Чому мікросервіси були болем до нього

#### 1. Теоретичне ядро
**.NET Aspire** — це сучасний хмарно-орієнтований (cloud-ready) стек для розробки та оркестрації розподілених застосунків у екосистемі .NET. Він покликаний вирішити три головні проблеми локальної та хмарної розробки:
1. **Оркестрація (Orchestration):** автоматичний узгоджений запуск проєктів, контейнерів та зовнішніх ресурсів.
2. **Інтеграція (Components / Integrations):** стандартизоване підключення баз даних, черг повідомлень та кешів з готовими налаштуваннями стійкості (resilience).
3. **Спостережуваність (Tooling & Observability):** вбудований дашборд із трасуванням (Distributed Tracing), структурованими логами та метриками з коробки без необхідності розгортати сторонні системи на зразок Jaeger чи Prometheus.

> 💡 **Життєва аналогія для чайників: Будівництво та прораб**  
> - **Раніше (до Aspire):** У вас є бригада сантехніків, електриків та малярів (ваші 3 мікросервіси). Кожен приходить зі своїм інструментом, вмикає власні подовжувачі (порти 5001, 5002, 5003), лається, якщо в розетці немає струму (БД ще не піднялась), а ви бігаєте між ними з рацією, намагаючись зрозуміти, чому не горить світло.
> - **З .NET Aspire:** У вас з'являється професійний **Прораб (AppHost)**. Він сам привозить генератор (піднімає RabbitMQ та SQL Server), чекає, поки він вийде на робочу потужність (`.WaitFor()`), подає живлення всім кімнатам і вивішує на планшеті єдиний статус-монітор (Aspire Dashboard), де видно кожен рух кожного майстра.

#### 2. Порівняльна таблиця: "Традиційний мікросервісний хаос" проти ".NET Aspire"

| Критерій | Традиційний підхід (Docker Compose + ручний запуск) | .NET Aspire Orchestration |
| :--- | :--- | :--- |
| **Точка входу** | Складний `docker-compose.yaml` + ручний запуск 3-х терміналів `dotnet run` | Один проєкт `CargoDelivery.AppHost` — натискання однієї клавіші `F5` |
| **Мережеві адреси та порти** | Хардкод у конфігах: `localhost:5001`, `localhost:1434`, `localhost:5672` | Динамічний **Service Discovery**: сервіси знаходять один одного за логічними іменами |
| **Телеметрія та логи** | 3 окремих вікна консолі + зовнішній Jaeger + Prometheus + Grafana | Єдиний вебінтерфейс **Aspire Dashboard** з логами, метриками та трасуванням OTLP |
| **Послідовність запуску** | Сервіс падає з помилкою `SqlException`, бо контейнер БД ще не стартував | Декларативний метод `.WaitFor(db)`, що чекає повної готовності залежності |
| **Конфігурація** | Десятки рядків YAML без автодоповнення і типобезпеки | 100% C#-код із підказками компілятора та повною валідацією типів |

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-2)

---

<a id="slide-2"></a>
### 🗺️ Слайд 2. Архітектурний ландшафт CargoDelivery: 3 API, gRPC, MassTransit та Інфраструктура

#### 1. Топологія розподіленої системи Блоку 2
Наша система складається з трьох самостійних бізнес-сервісів і виділеного HTTP-агрегатора, які обмінюються даними як синхронно (REST/JSON і високошвидкісний бінарний gRPC), так і асинхронно (Pub/Sub шина MassTransit RabbitMQ):

```mermaid
flowchart TB
    subgraph AspireDashboard ["📊 .NET Aspire Dashboard (OTLP Receiver)"]
        DashUI["Telemetry: Traces • Logs • Metrics • Health"]
    end

    subgraph AppHostContainer ["🎯 CargoDelivery.AppHost (Orchestrator)"]
        direction TB

        subgraph Infra ["🐳 Docker Containers (Керуються Aspire)"]
            RMQ[("🐰 RabbitMQ 4.x (AMQP + Management UI)")]
            SQL[("💾 MS SQL Server 2022")]
            DB1[("dev_driversmanagement")]
            DB2[("dev_driversblogmanagement")]
            DB3[("dev_cargoorderingservice")]
            SQL --> DB1
            SQL --> DB2
            SQL --> DB3
        end

        subgraph Services ["⚡ Microservice Web APIs (.NET 10)"]
            DM["🚚 DriversManagement<br/>(Core API & Gateway Aggregator)"]
            DB["📝 DriversBlogManagement<br/>(Blog & Feed API)"]
            CO["📦 CargoOrderingService<br/>(Orders API)"]
            AGG["🛡️ CargoDelivery.HttpAggregator<br/>(Gateway Aggregator)"]
        end
    end

    AGG -. "HTTP Scatter-Gather" .-> DM
    AGG -. "HTTP Scatter-Gather" .-> DB
    AGG -. "HTTP Scatter-Gather" .-> CO

    DM -. "1. gRPC Protobuf (HTTP/2)" .-> DB
    DM -. "2. HTTP Aggregator (BFF Task.WhenAll)" .-> DB
    DM -. "2. HTTP Aggregator (BFF Task.WhenAll)" .-> CO
    DM ==>|3. IDriverCreated / IDriverUpdated| RMQ
    RMQ ==>|Consume IDriverCreated| DB
    RMQ ==>|Consume IDriverCreated| CO
    CO ==>|"4. IOrderCreated (підписників поки немає)"| RMQ

    DM --> DB1
    DB --> DB2
    CO --> DB3

    DM -. "OTLP (gRPC)" .-> AspireDashboard
    DB -. "OTLP (gRPC)" .-> AspireDashboard
    CO -. "OTLP (gRPC)" .-> AspireDashboard
    AGG -. "OTLP (gRPC)" .-> AspireDashboard
```

#### 2. Прямі посилання на ключові компоненти та вихідний код

1. **Сервіс водіїв та Gateway Aggregator (BFF):**
   > 📍 **Шлях до файлу:** `02-Microservices-Craftsman/DriversManagement/src/DriversManagement/DriversManagement.csproj`  
   *Головний агрегат водія, CQRS-команди, обчислення метрик та шлюз агрегації даних.*

   [👉 **Перейти до файлу (клікніть тут)**](../DriversManagement/src/DriversManagement/DriversManagement.csproj)

2. **BFF Контролер агрегації дашборду (Task.WhenAll Fan-Out / Fan-In):**
   > 📍 **Шлях до файлу:** `02-Microservices-Craftsman/DriversManagement/src/DriversManagement/Controllers/v1/DriverDashboardController.cs`  
   *Композиція досьє водія, активних доставок та останніх блог-постів у єдиний DTO-відповідь.*

   [👉 **Перейти до файлу (клікніть тут)**](../DriversManagement/src/DriversManagement/Controllers/v1/DriverDashboardController.cs)

3. **Сервіс блогу водіїв (gRPC Сервер та стрічка дописів):**
   > 📍 **Шлях до файлу:** `02-Microservices-Craftsman/DriversBlogManagement/src/DriversBlogManagement/DriversBlogManagement.csproj`  
   *Соціальний функціонал, коментарі, вподобайки та високошвидкісний бінарний gRPC-ендпоінт.*

   [👉 **Перейти до файлу (клікніть тут)**](../DriversBlogManagement/src/DriversBlogManagement/DriversBlogManagement.csproj)

4. **Сервіс замовлень вантажів (Cargo Logistics):**
   > 📍 **Шлях до файлу:** `02-Microservices-Craftsman/CargoOrderingService/src/CargoOrderingService/CargoOrderingService.csproj`  
   *Управління замовленнями, рейсами, статусами доставки та фонова синхронізація водіїв.*

   [👉 **Перейти до файлу (клікніть тут)**](../CargoOrderingService/src/CargoOrderingService/CargoOrderingService.csproj)

5. **Виділений Gateway Aggregator (Scatter-Gather + лабораторія Polly):**
   > 📍 **Шлях до файлу:** `02-Microservices-Craftsman/CargoDelivery.HttpAggregator/Program.cs`
   *Типізовані HTTP-клієнти з Service Discovery, паралельний Fan-Out, Graceful Degradation, демо Retry/Timeout/Circuit Breaker.*

   [👉 **Перейти до файлу (клікніть тут)**](../CargoDelivery.HttpAggregator/Program.cs)

6. **Спільні контракти повідомлень шини (SharedKernel):**
   > 📍 **Шлях до файлу:** `02-Microservices-Craftsman/SharedKernel/SharedKernel.csproj`  
   *Інтерфейси доменних подій IDriverCreated, IDriverUpdated, IOrderCreated.*

   [👉 **Перейти до файлу (клікніть тут)**](../SharedKernel/SharedKernel.csproj)

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-3)

---

<a id="slide-3"></a>
### 🎻 Слайд 3. Проєкт CargoDelivery.AppHost — C# як диригент оркестру замість Docker Compose

#### 1. Теоретичне ядро
`AppHost` — це консольний проєкт на базі SDK `Aspire.AppHost.Sdk`. Він **не містить** бізнес-логіки. Його єдина роль — бути моделлю розподіленого застосунку (Application Model).  
Замість написання нетипізованого YAML-файлу `docker-compose.yml`, конфігурація задається безпосередньо у C#-коді:
- Проєкти реєструються через `builder.AddProject<Projects.MyService>("name")`.
- Контейнери інфраструктури підключаються через розширення `builder.AddRabbitMQ()`, `builder.AddSqlServer()`.
- Зв'язки між сервісами описуються через `.WithReference()`.

> 💡 **Життєва аналогія: Партитура диригента**  
> `AppHost` — це нотна партитура. Диригент не грає сам на скрипці чи барабанах (це роблять мікросервіси), але він точно знає, коли вступає ударна установка (база даних), а коли соло бере саксофон (Web API).

#### 2. Реальний код нашого AppHost
> 📍 **Шлях до файлу:** `02-Microservices-Craftsman/Aspire/CargoDelivery.AppHost/AppHost.cs`

[👉 **Перейти до файлу (клікніть тут)**](../Aspire/CargoDelivery.AppHost/AppHost.cs)

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// 1. Інфраструктура (Full Aspire Orchestration): RabbitMQ з веб-інтерфейсом
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// 2. Інфраструктура: MS SQL Server та окремі бази даних для кожного мікросервісу.
//    Ім'я ресурсу ("*-db") ≠ фізична назва БД ("dev_*"): імена ресурсів мають бути
//    унікальними без урахування регістру і не збігатися з іменами проєктів.
var sqlserver = builder.AddSqlServer("sqlserver");
var driversDb = sqlserver.AddDatabase("driversmanagement-db", "dev_driversmanagement");
var blogDb = sqlserver.AddDatabase("driversblogmanagement-db", "dev_driversblogmanagement");
var cargoDb = sqlserver.AddDatabase("cargoorderingservice-db", "dev_cargoorderingservice");

// 3. Мікросервіс замовлень вантажів (MassTransit паблішер і консьюмер)
var cargoApi = builder.AddProject<Projects.CargoOrderingService>("cargoorderingservice")
    .WithReference(cargoDb, connectionName: "CargoOrderingService") // → ConnectionStrings__CargoOrderingService
    .WithReference(rabbitmq)
    .WithHttpHealthCheck("/health")                                 // готовність = GET /health → 200
    .WaitFor(cargoDb)
    .WaitFor(rabbitmq);

// 4. Мікросервіс блогу водіїв (gRPC сервер + MassTransit консьюмер)
var blogApi = builder.AddProject<Projects.DriversBlogManagement>("driversblogmanagement")
    .WithReference(blogDb, connectionName: "DriversBlogManagement")
    .WithReference(rabbitmq)
    .WithHttpHealthCheck("/health")
    .WaitFor(blogDb)
    .WaitFor(rabbitmq);

// 5. Головний сервіс водіїв + вбудований BFF (gRPC клієнт + HTTP BFF + MassTransit паблішер)
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

// 6. Виділений Gateway Aggregator
var httpAggregator = builder.AddProject<Projects.CargoDelivery_HttpAggregator>("httpaggregator")
    .WithReference(driversApi)
    .WithReference(cargoApi)
    .WithReference(blogApi)
    .WithHttpHealthCheck("/health")
    .WaitFor(driversApi)
    .WaitFor(cargoApi)
    .WaitFor(blogApi);

builder.Build().Run();
```

#### 3. Що відбувається під капотом?
1. `Projects.DriversManagement` — це згенерований генератором коду (Source Generator) статичний типізований клас, який дивиться на реальний `.csproj` файл. Якщо файл проєкту буде перейменовано або переміщено, помилка виникне під час компіляції, а не в рантаймі!
2. `.WithReference(driversDb, connectionName: "DriversManagement")` автоматично інжектує у змінну середовища процесу рядок підключення:
   `ConnectionStrings__DriversManagement=Server=127.0.0.1,{порт};...;Database=dev_driversmanagement`
3. `.WithReference(blogApi)` та `.WithReference(cargoApi)` автоматично додають адреси кінцевих точок у середовище `driversmanagement`:
   `services__driversblogmanagement__https__0=https://localhost:5002`
   `services__cargoorderingservice__https__0=https://localhost:5003`
4. Карта рядків реального файлу з поясненнями — у [гайді 01, §4.2](01_Aspire_Orchestration_AppHost_DeepDive.md#section-4).

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-4)

---

<a id="slide-4"></a>
### 📦 Слайд 4. Full Aspire Orchestration: Декларативний запуск RabbitMQ та SQL Server з коду

#### 1. Теоретичне ядро
У режимі **Full Aspire Orchestration** розробнику більше не потрібно попередньо піднімати окремі екземпляри Docker-контейнерів через зовнішні батники чи скрипти. Aspire взаємодіє з Docker Desktop / Podman безпосередньо:
- Метод `.AddRabbitMQ("rabbitmq").WithManagementPlugin()` наказує Aspire завантажити офіційний образ `rabbitmq:4.x-management` та опублікувати ендпоінти AMQP (у контейнері `5672`) і веб-панелі управління (у контейнері `15672`) на **динамічних** портах хоста. Пароль користувача `guest` Aspire генерує сам (параметр `rabbitmq-password`).
- Метод `.AddSqlServer("sqlserver")` створює контейнер Microsoft SQL Server (`mcr.microsoft.com/mssql/server:2022-latest`), генерує безпечний пароль SA для локальної розробки та створює логічні бази даних через виклики `.AddDatabase(...)`.

```mermaid
sequenceDiagram
    autonumber
    actor Dev as Розробник (F5)
    participant Host as CargoDelivery.AppHost
    participant Engine as Docker Engine
    participant Dash as Aspire Dashboard
    participant API as Microservice API

    Dev->>Host: dotnet run
    Host->>Dash: Ініціалізація OTLP Dashboard
    Host->>Engine: docker run rabbitmq:4.x-management
    Host->>Engine: docker run mcr.microsoft.com/mssql/server
    Note over Host,Engine: Контейнери стартують у бекграунді
    Host->>API: Старт процесу (Kestrel) з інжектованими ConnectionStrings
    API->>Dash: Відправка логів, метрик та трейсів через OTLP
```

#### 2. Посилання на код
> 📍 **Шлях до файлу:** `02-Microservices-Craftsman/Aspire/CargoDelivery.AppHost/AppHost.cs`

[👉 **Перейти до файлу (клікніть тут)**](../Aspire/CargoDelivery.AppHost/AppHost.cs)

#### 3. Чому це зручно?
1. **Ізоляція баз даних:** Кожен мікросервіс отримує власну базу даних (`dev_driversmanagement`, `dev_driversblogmanagement`, `dev_cargoorderingservice`), що повністю відповідає принципу **Database-per-Service** у Domain-Driven Design (DDD).
2. **Нуль налаштувань у appsettings.json:** Розробнику, який щойно склонував репозиторій, не потрібно налаштовувати паролі від SQL Server чи адреси хостів — Aspire згенерує і з'єднає все на льоту!
3. **Ефемерність:** контейнери створюються заново при кожному запуску (без томів даних), тому схему БД сервіси створюють самі при старті (`MigrateAsync` або `EnsureCreatedAsync` — [MigrationHostedService.cs#L48](../DriversManagement/src/DriversManagement/Databases/MigrationHostedService.cs#L48)). Детальніше про «старі бази» і `WithDataVolume()` — [гайд 01, §9](01_Aspire_Orchestration_AppHost_DeepDive.md#section-9).

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-5)

---

<a id="slide-5"></a>
### ⏱️ Слайд 5. Патерн .WaitFor(): Подолання Race Conditions при старті розподілених систем

#### 1. Теоретичне ядро
Одвічна біда мікросервісів при локальному запуску — **стан гонитви (Race Condition)**.  
Коли ви запускаєте проєкт через `docker-compose up`, контейнер з базою даних SQL Server може підніматися 10-15 секунд, ініціалізуючи файли `.mdf`/`.ldf`. Тим часом Web API на .NET стартує за 1 секунду, намагається виконати міграції EF Core (`context.Database.Migrate()`) і миттєво падає з помилкою:  
`Microsoft.Data.SqlClient.SqlException: A network-related or instance-specific error occurred`.

> 💡 **Життєва аналогія: Поїзд та рейки**  
> Ви не пускаєте швидкісний потяг (Web API), доки залізничники не закінчили вкладати рейки (SQL Server). Патерн `.WaitFor()` — це семафор, який тримає потяг на станції до моменту, поки рейки не пройдуть перевірку надійності.

#### 2. Як це вирішено в нашому AppHost
> 📍 **Шлях до файлу:** `02-Microservices-Craftsman/Aspire/CargoDelivery.AppHost/AppHost.cs`

[👉 **Перейти до файлу (клікніть тут)**](../Aspire/CargoDelivery.AppHost/AppHost.cs)

```csharp
var driversApi = builder.AddProject<Projects.DriversManagement>("driversmanagement")
    .WithReference(driversDb)
    .WithReference(rabbitmq)
    .WithReference(blogApi)
    .WithReference(cargoApi)
    .WaitFor(driversDb)    // ⏳ Чекати, доки підніметься SQL Server та база даних
    .WaitFor(rabbitmq)     // ⏳ Чекати, доки брокер RabbitMQ буде готовий приймати TCP-з'єднання
    .WaitFor(blogApi)      // ⏳ Чекати, доки сервіс блогу стане Healthy для gRPC-викликів
    .WaitFor(cargoApi);    // ⏳ Чекати, доки сервіс замовлень стане Healthy для HTTP BFF-викликів
```

> ⚠️ Для **проєктів** «Healthy» означає щось лише тоді, коли в AppHost додано `.WithHttpHealthCheck("/health")` (у нас — для всіх чотирьох сервісів). Без нього Aspire вважає проєкт здоровим, щойно процес запущено, і `WaitFor` не захищає від `Connection refused` на старті.

#### 3. Що бачить користувач у Aspire Dashboard?
У вебінтерфейсі стан сервісу `driversmanagement` послідовно змінюється:
1. `Waiting` — сервіс чекає на залежності з `.WaitFor(...)` (`driversmanagement-db`, `rabbitmq`, `driversblogmanagement`, `cargoorderingservice`).
2. `Starting` — як тільки залежності успішно пройшли перевірку готовності.
3. `Running` + Health `Healthy` (зелений індикатор) — мікросервіс відповів 200 на `/health` і готовий приймати зовнішні HTTP-запити.

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-6)

---

<a id="slide-6"></a>
### 🛠️ Слайд 6. Проєкт CargoDelivery.ServiceDefaults: Універсальний "швейцарський ніж" мікросервісу

#### 1. Теоретичне ядро
Проєкт `CargoDelivery.ServiceDefaults` містить спільні розширення конфігурації для кожного мікросервісу системи. Замість того, щоб копіювати десятки рядків коду налаштування OpenTelemetry, HealthChecks та Service Discovery у кожен окремий `Program.cs`, мікросервіс викликає лише **один метод-розширення**:

```csharp
builder.AddServiceDefaults();
```

А в пайплайні HTTP-запитів додає стандартні діагностичні ендпоінти:
```csharp
app.MapDefaultEndpoints();
```

```mermaid
flowchart LR
    SD["📦 CargoDelivery.ServiceDefaults"]
    
    SD --> OTEL["🔍 OpenTelemetry<br/>(Metrics, Traces, Logs via OTLP)"]
    SD --> HC["💓 Health Checks<br/>(/health ready, /alive live)"]
    SD --> SDISC["🔎 Service Discovery<br/>(Microsoft.Extensions.ServiceDiscovery)"]
    SD --> RES["🛡️ HTTP Resilience<br/>(Polly v8 Standard Resilience)"]

    DM["🚚 DriversManagement"] -->|AddServiceDefaults| SD
    DB["📝 DriversBlogManagement"] -->|AddServiceDefaults| SD
    CO["📦 CargoOrderingService"] -->|AddServiceDefaults| SD
    AGG["🛡️ HttpAggregator"] -->|AddServiceDefaults| SD
```

#### 2. Посилання на код

1. **Файл спільних розширень ServiceDefaults:**
   > 📍 **Шлях до файлу:** `02-Microservices-Craftsman/Aspire/CargoDelivery.ServiceDefaults/Extensions.cs`  
   *Конфігурація метрик, логування, трейсингу, Polly та Health Checks.*

   [👉 **Перейти до файлу (клікніть тут)**](../Aspire/CargoDelivery.ServiceDefaults/Extensions.cs)

2. **Реєстрація в мікросервісі водіїв:**
   > 📍 `builder.AddServiceDefaults()` викликається через обгортку [OpenTelemetryServiceExtension.cs#L20](../DriversManagement/src/DriversManagement/Extensions/Services/OpenTelemetryServiceExtension.cs#L20), а `app.MapDefaultEndpoints()` — у [Program.cs#L57](../DriversManagement/src/DriversManagement/Program.cs#L57).
   *У Program.cs також є важливе виправлення для Serilog (`WriteTo.Providers`) — без нього логи сервісу не потрапляли у Structured Logs дашборду ([Program.cs#L23](../DriversManagement/src/DriversManagement/Program.cs#L23)).*

   [👉 **Перейти до файлу (клікніть тут)**](../DriversManagement/src/DriversManagement/Program.cs)

#### 3. Що під капотом методу `AddServiceDefaults()`?
1. **`builder.ConfigureOpenTelemetry()`**: конфігурує збір метрик процесу, AspNetCore, HttpClient та експорт через OTLP.
2. **`builder.AddDefaultHealthChecks()`**: додає перевірку працездатності (Liveness probe).
3. **`builder.Services.AddServiceDiscovery()`**: дозволяє звертатися за назвами сервісів (`https+http://driversblogmanagement`).
4. **`http.AddStandardResilienceHandler()`**: активує механізм повторних спроб (retry), таймаутів та автоматичного розмикання ланцюга (circuit breaker) для **всіх** HttpClient і gRPC-клієнтів.

Повний розбір — [гайд 02](02_ServiceDefaults_Architecture_And_Parameters.md).

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-7)

---

<a id="slide-7"></a>
### 📉 Слайд 7. Прощавай, Jaeger! Перехід на нативний OTLP Pipeline у .NET Aspire Dashboard

#### 1. Теоретичне ядро
Раніше у генераторі Craftsman для розподіленого трасування використовувався пакет `OpenTelemetry.Exporter.Jaeger`:
```xml
<!-- СТАРИЙ ПІДХІД (Устарілий та важкий): -->
<PackageReference Include="OpenTelemetry.Exporter.Jaeger" Version="1.5.1" />
```
У цьому підході кожен сервіс мав захардкоджений порт агента Jaeger (`56390` чи `57202`), вимагав окремого контейнера `jaegertracing/all-in-one` у `docker-compose.yaml` та застарілого UDP/Compact Thrift протоколу передачі даних. Сам проєкт Jaeger офіційно оголосив цей експортер **Deprecated** на користь стандарту **OTLP (OpenTelemetry Protocol)**.

#### 2. Сучасний підхід з .NET Aspire: Нативний OTLP
У .NET Aspire все трасування, метрики та логи передаються за єдиним відкритим протоколом **OTLP** поверх gRPC / HTTP:

> 📍 **Шлях до файлу:** `02-Microservices-Craftsman/Aspire/CargoDelivery.ServiceDefaults/Extensions.cs`

[👉 **Перейти до файлу (клікніть тут)**](../Aspire/CargoDelivery.ServiceDefaults/Extensions.cs)

```csharp
private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
{
    var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

    if (useOtlpExporter)
    {
        builder.Services.AddOpenTelemetry().UseOtlpExporter();
    }

    return builder;
}
```

#### 3. Порівняння архітектури трасування

```mermaid
flowchart TD
    subgraph OldWay ["❌ Старий підхід (Jaeger Thrift)"]
        A1["Microservice"] -->|"UDP Thrift (порт 56390)"| J1["Jaeger Agent"]
        J1 --> J2["Jaeger Collector"]
        J2 --> J3["Jaeger UI (порт 16686)"]
    end

    subgraph NewWay ["✅ Новий підхід Aspire (OTLP Native)"]
        B1["Microservice"] -->|"OTLP over gRPC (Стандарт W3C TraceContext)"| D1["🎯 Aspire Dashboard"]
        D1 -->|"Вбудований інтерфейс (Трейси + Логи + Метрики)"| D2["Browser UI"]
    end
```

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-8)

---

<a id="slide-8"></a>
### 📬 Слайд 8. Distributed Tracing для MassTransit + RabbitMQ: Сквозний трейсинг EDA-подій

#### 1. Теоретичне ядро
У подієво-орієнтованих архітектурах (Event-Driven Architecture) класичне логування безсиле: коли `DriversManagement` публікує подію `IDriverCreated` у RabbitMQ, HTTP-запит клієнта вже завершився з кодом `201 Created`. Споживач (`DriversBlogManagement`) опрацює повідомлення асинхронно через кілька мілісекунд в іншому процесі.  
Як побачити зв'язок між цими операціями? **Сквозний розподілений трейсинг (Trace Context Propagation)**!

MassTransit підтримує стандарт W3C `traceparent`. Коли повідомлення відправляється в чергу, MassTransit додає заголовки трасування. Коли консьюмер вичитує повідомлення, він підхоплює цей Trace ID!

#### 2. Як ми навчили Aspire бачити події MassTransit?
У нашому розширенні `Extensions.cs` ми зареєстрували джерело активності MassTransit:

> 📍 **Шлях до файлу:** `02-Microservices-Craftsman/Aspire/CargoDelivery.ServiceDefaults/Extensions.cs`

[👉 **Перейти до файлу (клікніть тут)**](../Aspire/CargoDelivery.ServiceDefaults/Extensions.cs)

```csharp
.WithTracing(tracing =>
{
    tracing.AddSource(builder.Environment.ApplicationName)
        .AddSource("MassTransit") // 👈 Магічний рядок для відстеження шини повідомлень!
        .AddAspNetCoreInstrumentation(...)
        .AddHttpClientInstrumentation()
        .AddSqlClientInstrumentation(); // 👈 SQL-запити EF Core як дочірні спани
});
```
> 📍 [Extensions.cs#L147](../Aspire/CargoDelivery.ServiceDefaults/Extensions.cs#L147). Раніше замість `AddSqlClientInstrumentation()` стояв `AddSource("Microsoft.EntityFrameworkCore.SqlServer")` — такого джерела не існує, тож SQL у трейсах не було.

#### 3. Візуалізація спану в Aspire Dashboard

```mermaid
gantt
    title Сквозний Trace: Створення водія та асинхронна реплікація
    dateFormat X
    axisFormat %s мс

    section DriversManagement (HTTP)
    POST /api/drivers (HTTP Controller) :active, p1, 0, 45
    EF Core INSERT INTO Drivers :p2, 10, 30
    MassTransit Publish IDriverCreated :p3, 30, 45

    section RabbitMQ Broker
    Routing via Fanout Exchange :crit, p4, 45, 55

    section DriversBlogManagement (Consumer)
    sync-updated-driver receive/process :active, p5, 55, 95
    SQL INSERT INTO drivers (тіньова копія) :p6, 65, 90

    section CargoOrderingService (Consumer)
    ordering-sync-driver receive/process :active, p7, 55, 100
    SQL INSERT INTO drivers (тіньова копія) :p8, 70, 95
```

У Aspire Dashboard ви бачите це дерево викликів як єдиний пов'язаний ланцюжок (на стенді: `POST api/drivers` → `send driver-created` → `ordering-sync-driver receive/process` і `sync-updated-driver receive/process`, разом ≈1 с). Клікнувши на спан, видно його атрибути (обмінник, черга, ідентифікатор повідомлення), тривалість обробки та дочірні SQL-запити. Детально — [гайд 04, §8](04_Distributed_Tracing_OpenTelemetry_Guide.md#section-8).

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-9)

---

<a id="slide-9"></a>
### ⚡ Слайд 9. Service Discovery на практиці: Як DriversManagement знаходить gRPC у DriversBlogManagement

#### 1. Теоретичне ядро
Раніше у файлі `appsettings.Development.json` для `DriversManagement` адреса gRPC-сервера була захардкоджена:
```json
"GrpcServer": {
  "Address": "https://localhost:5002"
}
```
Якщо інший розробник змінював порт, сервіс блогу запускався на випадковому порту в Docker або в продакшні адреса змінювалася, виклики gRPC падали з помилкою `RpcException: Unavailable`.

#### 2. Реалізація Service Discovery з автоматичним Fallback
> 📍 **Шлях до файлу:** `02-Microservices-Craftsman/DriversManagement/src/DriversManagement/Extensions/Services/WebAppServiceConfiguration.cs`

[👉 **Перейти до файлу (клікніть тут)**](../DriversManagement/src/DriversManagement/Extensions/Services/WebAppServiceConfiguration.cs)

```csharp
// Реєстрація gRPC-клієнта з підтримкою .NET Aspire Service Discovery
builder.Services.AddGrpcClient<DriverRpc.DriverRpcClient>(options =>
{
    // Під Aspire: логічне ім'я, яке резолвить Service Discovery (services:driversblogmanagement:https:0).
    // Без Aspire: адреса з appsettings (GrpcServer:Address).
    var grpcAddress = builder.Configuration["services:driversblogmanagement:https:0"] is not null
        ? "https://driversblogmanagement"
        : builder.Configuration.GetSection("GrpcServer")["Address"] ?? "https://localhost:5002";

    options.Address = new Uri(grpcAddress);
})
.RemoveAllResilienceHandlers()
.AddStandardResilienceHandler(SoftDependencyBudget); // короткий бюджет: пости — некритичні дані
```
> 📍 [WebAppServiceConfiguration.cs#L58](../DriversManagement/src/DriversManagement/Extensions/Services/WebAppServiceConfiguration.cs#L58)

#### 3. Як це працює по кроках:
1. Коли проєкт запускається всередині Aspire AppHost через зв'язку `.WithReference(blogApi)`, Aspire передає адресу сервісу блогу у змінну середовища:
   `services__driversblogmanagement__https__0 = https://localhost:5002`.
2. `AddGrpcClient` будує клієнт через `IHttpClientFactory`, тому на нього діють глобальні налаштування ServiceDefaults: обробник Service Discovery підміняє `https://driversblogmanagement` на реальну адресу.
3. Якщо сервіс запускається автономно (без Aspire), код безпечно повертається до значення з `appsettings.json` або дефолтного порту `https://localhost:5002`.
4. gRPC вимагає HTTP/2 — тому використовується саме HTTPS-ендпоінт (HTTP/2 узгоджується через TLS ALPN).

> 🧩 **Contract-first.** Клієнт генерується з того самого `Driver.proto`, що й сервер (`<Protobuf ... GrpcServices="Client" />` у DriversManagement.csproj). Раніше DriversManagement посилався на **весь проєкт** блогу, через що в його процес потрапляли чужі контролери (`/api/drivers` давав `AmbiguousMatchException`). Деталі — [03_Patterns_Synchronous_gRPC_Communication.md](../02-02-Communication-Documentation/03_Patterns_Synchronous_gRPC_Communication.md).

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-10)

---

<a id="slide-10"></a>
### 🔌 Слайд 10. Інтелектуальний зв'язок з RabbitMQ: Гібридний fallback (Aspire vs appsettings vs In-Memory)

#### 1. Теоретичне ядро
Коли мікросервіси запускаються в різних середовищах (локально в Aspire, в юніт-тестах, у Docker чи на CI/CD), вони отримують конфігурацію брокера повідомлень у різний спосіб:
- В **Aspire** рядок підключення передається у стандартному форматі: `ConnectionStrings:rabbitmq = amqp://guest:{згенерований_пароль}@localhost:{динамічний_порт}`.
- У **класичному запуску Craftsman** параметри розбиті по секціях: `RabbitMq:Host`, `RabbitMq:Port`, `RabbitMq:Username` тощо.
- В **тестах** зовнішнього брокера немає взагалі: у DriversManagement і блозі шина в тестових середовищах не реєструється, а CargoOrderingService має fallback на `In-Memory Bus`.

#### 2. Універсальний адаптер у MassTransitServiceExtension
> 📍 **Шлях до файлу:** `02-Microservices-Craftsman/DriversManagement/src/DriversManagement/Extensions/Services/MassTransitServiceExtension.cs`

[👉 **Перейти до файлу (клікніть тут)**](../DriversManagement/src/DriversManagement/Extensions/Services/MassTransitServiceExtension.cs)

```csharp
var rmqOptions = configuration.GetRabbitMqOptions();
var rmqConnectionString = configuration.GetConnectionString("rabbitmq");

services.AddMassTransit(mt =>
{
    mt.AddConsumers(Assembly.GetExecutingAssembly());
    mt.UsingRabbitMq((context, cfg) =>
    {
        // 1. Пріоритет №1: Підключення від .NET Aspire
        if (!string.IsNullOrEmpty(rmqConnectionString))
        {
            var uri = new Uri(rmqConnectionString);
            var userInfo = uri.UserInfo.Split(':');
            // Uri.UserInfo — percent-encoded: згенерований пароль може містити спецсимволи
            var username = userInfo.Length > 0 && !string.IsNullOrEmpty(userInfo[0]) ? Uri.UnescapeDataString(userInfo[0]) : "guest";
            var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "guest";
            var vhost = string.IsNullOrEmpty(uri.AbsolutePath.Trim('/')) ? "/" : uri.AbsolutePath.Trim('/');

            cfg.Host(uri.Host, uri.Port > 0 ? (ushort)uri.Port : (ushort)5672, vhost, h =>
            {
                h.Username(username);
                h.Password(password);
            });
        }
        // 2. Пріоритет №2: Класична конфігурація з appsettings.Development.json
        else if (rmqOptions != null && !string.IsNullOrEmpty(rmqOptions.Host))
        {
            cfg.Host(rmqOptions.Host, 
                ushort.TryParse(rmqOptions.Port, out var p) ? p : (ushort)5672, 
                string.IsNullOrEmpty(rmqOptions.VirtualHost) ? "/" : rmqOptions.VirtualHost, 
                h =>
                {
                    h.Username(string.IsNullOrEmpty(rmqOptions.Username) ? "guest" : rmqOptions.Username);
                    h.Password(string.IsNullOrEmpty(rmqOptions.Password) ? "guest" : rmqOptions.Password);
                });
        }
    });
});
```

> 📍 Реальний код: [MassTransitServiceExtension.cs#L36](../DriversManagement/src/DriversManagement/Extensions/Services/MassTransitServiceExtension.cs#L36)

#### 3. Чому це надійно?
Жодна зміна конфігурації не зламала попередні тести чи ручний запуск: якщо Aspire активний — працює Aspire-контейнер, якщо запущено без нього — сервіс використовує параметри з `appsettings.json`. Пріоритет простий: змінні середовища (від Aspire) перекривають appsettings.

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-11)

---

<a id="slide-11"></a>
### 💓 Слайд 11. Health Checks та Probes: Анатомія ендпоінтів /health та /alive

#### 1. Теоретичне ядро
Для забезпечення надійності в сучасних оркестраторах (Kubernetes, Docker Swarm, Azure Container Apps, .NET Aspire) застосунок повинен відповідати на два окремих запитання:
1. **Liveness Probe (`/alive`):** *"Ти живий?"* — чи процес не завис намертво, чи відповідає HTTP-сервер. Якщо запит падає, оркестратор перезапускає процес.
2. **Readiness Probe (`/health`):** *"Ти готовий приймати клієнтські запити?"* — чи підключився застосунок до бази даних, брокера повідомлень і зовнішніх сервісів. Якщо ні, трафік на нього тимчасово не направляється.

```mermaid
flowchart TD
    Req["Зовнішній HTTP-запит"] --> Router{Orchestrator Router}
    
    subgraph HealthChecks ["Діагностичні ендпоінти Aspire"]
        direction TB
        HC["/health (Readiness)"]
        AL["/alive (Liveness)"]
    end

    HC -->|Перевіряє| Self2["self (зараз)"]
    HC -.->|Рекомендоване розширення| DBCheck["З'єднання з SQL Server"]
    HC -.->|Рекомендоване розширення| RMQCheck["З'єднання з RabbitMQ"]
    AL -->|Перевіряє| Self["Тільки внутрішній статус процесу"]

    Router -->|Якщо /health == 200 OK| Service["Робочий мікросервіс"]
    Router -.->|Якщо /health != 200 OK| Fallback["Трафік блокується (Сервіс ще прогрівається)"]
```

#### 2. Посилання на код
> 📍 **Шлях до файлу:** `02-Microservices-Craftsman/Aspire/CargoDelivery.ServiceDefaults/Extensions.cs`

[👉 **Перейти до файлу (клікніть тут)**](../Aspire/CargoDelivery.ServiceDefaults/Extensions.cs)

```csharp
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        // Готовність приймати трафік (усі перевірки пройшли успішно)
        app.MapHealthChecks("/health");

        // Життєздатність (процес живий)
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });
    }

    return app;
}
```
> 📍 [Extensions.cs#L248](../Aspire/CargoDelivery.ServiceDefaults/Extensions.cs#L248). Саме `/health` опитує Aspire завдяки `.WithHttpHealthCheck("/health")` в AppHost. Детально — [гайд 05](05_HealthChecks_Probes_Liveness_Readiness.md).

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-12)

---

<a id="slide-12"></a>
### 🛡️ Слайд 12. Resilience & Polly у ServiceDefaults: Стандартна стійкість HTTP-клієнтів

#### 1. Теоретичне ядро
У мікросервісах будь-який міжсервісний HTTP-виклик може тимчасово дати збій: короткочасний сплеск мережі, рестарт по сусідству, секундна затримка DNS. Без автоматичного захисту клієнт відразу отримає помилку `500 Internal Server Error`.  
У .NET Aspire вбудовано пакет `Microsoft.Extensions.Http.Resilience` на базі бібліотеки **Polly v8**, що автоматично конфігурує **Standard Resilience Pipeline**:

```mermaid
flowchart LR
    Request["HTTP Запит"] --> RateLimiter["1. Rate Limiter<br/>(Захист від перевантаження)"]
    RateLimiter --> TimeoutTotal["2. Total Timeout<br/>(Глобальний ліміт: 30с)"]
    TimeoutTotal --> Retry["3. Retry Policy<br/>(3 спроби з експоненційною затримкою)"]
    Retry --> CircuitBreaker["4. Circuit Breaker<br/>(Захист від лавинних аварій)"]
    CircuitBreaker --> TimeoutAttempt["5. Attempt Timeout<br/>(Ліміт спроби: 10с)"]
    TimeoutAttempt --> Network["Зовнішній мікросервіс"]
```

#### 2. Реалізація у нашому ServiceDefaults
> 📍 **Шлях до файлу:** `02-Microservices-Craftsman/Aspire/CargoDelivery.ServiceDefaults/Extensions.cs`

[👉 **Перейти до файлу (клікніть тут)**](../Aspire/CargoDelivery.ServiceDefaults/Extensions.cs)

```csharp
builder.Services.ConfigureHttpClientDefaults(http =>
{
    // Вмикає стандартний конвеєр відмовостійкості Polly для всіх HttpClient
    http.AddStandardResilienceHandler();

    // Вмикає Service Discovery за замовчуванням
    http.AddServiceDiscovery();
});
```

#### 3. Що це дає системі?
Якщо `HttpAggregator` надсилає запит до `DriversManagement`, а той у цей момент повертає 503, Polly автоматично:
1. Зачекає (за замовчуванням ≈2 с з jitter, далі експоненційно) і повторить запит (Retry).
2. Якщо сервіс остаточно перестав відповідати — розімкне ланцюг (Circuit Breaker), захищаючи систему від зависання потоків.
3. Обмежить кожну спробу 10 с, а весь виклик — 30 с.

> ⚠️ Не додавайте `.AddStandardResilienceHandler()` до окремих клієнтів поверх глобального — конвеєри вкладуться. Для зміни параметрів — `RemoveAllResilienceHandlers()` + власні опції. Живі експерименти Retry / Timeout / Circuit Breaker — [гайд 06](06_ServiceDiscovery_Resilience_Polly_HttpClient.md).

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-13)

---

<a id="slide-13"></a>
### 🖥️ Слайд 13. Aspire Dashboard: Детальний гайд по інтерфейсу телеметрії (Logs, Traces, Metrics)

#### 1. Структура панелей Aspire Dashboard
При старті `CargoDelivery.AppHost` відкривається сучасний веб-дашборд — у нашому проєкті `https://localhost:17048` (адреса задана в [launchSettings.json AppHost](../Aspire/CargoDelivery.AppHost/Properties/launchSettings.json#L8); у консолі друкується URL з токеном входу):

```mermaid
flowchart TD
    subgraph DashboardViews ["📊 Основні розділи Aspire Dashboard"]
        R["📦 Resources (Ресурси)"]
        CL["📜 Console Logs (Консольні логи)"]
        SL["📑 Structured Logs (Структуровані логи)"]
        TR["⚡ Traces (Розподілені трейси)"]
        ME["📈 Metrics (Метрики застосунків)"]
    end

    R -->|Відображає| RInfo["Статус процесів: Running, Finished, Health Checks, Endpoints"]
    CL -->|Відображає| CLInfo["Сирий вивід stdout/stderr кожного мікросервісу та контейнера"]
    SL -->|Відображає| SLInfo["ILogger / Serilog (через WriteTo.Providers) події з фільтрацією за рівнем, ресурсом, TraceId"]
    TR -->|Відображає| TRInfo["Gantt-діаграми ланцюжків HTTP ➔ gRPC ➔ RabbitMQ ➔ SQL"]
    ME -->|Відображає| MEInfo["CPU, Memory, GC allocations, Active HTTP requests, Thread pool"]
```

#### 2. Як використовувати розробнику:
1. **Вкладка Resources:**  
   Клікніть на посилання endpoint'у біля `driversmanagement` (наприклад, `https://localhost:5001/swagger`) — ви одразу потрапляєте в Swagger UI сервісу.
2. **Вкладка Traces:**
   Зробіть виклик через Swagger або [CargoDelivery.HttpAggregator.http](../CargoDelivery.HttpAggregator/CargoDelivery.HttpAggregator.http) `POST /api/drivers`. Перейдіть у Traces: ви побачите єдиний рядок трейсу. Відкривши його, ви побачите ієрархію: виклик контролера ➔ збереження в SQL ➔ публікація в RabbitMQ ➔ асинхронний прийом у блозі та в сервісі замовлень.
3. **Вкладка Structured Logs:**
   Швидкий пошук за будь-яким `DriverId` чи винятком (спробуйте ввести `MassTransit` у фільтр). Натискання на Trace ID у лозі миттєво переносить вас у вікно детального трейсу цієї операції!
4. **Кнопки ■ / ▶ у колонці Actions:** зупиніть `driversblogmanagement` і подивіться, як агрегатор переходить у режим `Partial (Degraded)` — це найкраща жива демонстрація стійкості.

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-14)

---

<a id="slide-14"></a>
### 🔄 Слайд 14. Міграція всієї підсистеми на .NET 10 та стабілізація стенду: 149/149 зелених тестів

#### 1. Передумови та технічні виклики
Початковий стан Блоку 2 мав суттєвий архітектурний розрив:
- `CargoOrderingService` вже був на .NET 10.
- `DriversManagement` та `DriversBlogManagement` залишалися на .NET 7 (який вже є End-of-Life).
- Проєкти використовували несумісні версії залежностей і застарілий Jaeger-клієнт.

#### 2. Що було зроблено під час модернізації:
1. **Оновлення TargetFramework:**  
   Усі 10 проєктів Блоку 2 переведено на `<TargetFramework>net10.0</TargetFramework>`.
2. **Оновлення NuGet-пакетів:**  
   - `Microsoft.EntityFrameworkCore`: 7.0.11 ➔ **8.0.0 / 10.0**
   - `Microsoft.AspNetCore.Authentication.JwtBearer`: 7.0.11 ➔ **8.0.0**
   - `MediatR`: 12.1.1 ➔ **12.2.0**
   - `Hangfire`: 1.8.5 ➔ **1.8.6**
   - `HeimGuard`: 0.3.0 ➔ **1.0.0**
   - `xunit`: 2.5.1 ➔ **2.6.4**
   - `Testcontainers`: 3.5.0 ➔ **3.6.0**
3. **Видалення застарілих компонентів:**  
   Повністю демонтовано `OpenTelemetry.Exporter.Jaeger`. Його функціонал замінено нативним OTLP через `CargoDelivery.ServiceDefaults`.
4. **Відмова від міжсервісного `ProjectReference`:**
   Раніше DriversManagement посилався на проєкт DriversBlogManagement (заради gRPC-типів) з `<PrivateAssets>all</PrivateAssets>`. Це тягнуло в процес чужі контролери (`AmbiguousMatchException` на `/api/drivers`). Тепер спільний лише контракт `Driver.proto`, який компілюється як `GrpcServices="Client"`.

#### 2.1. Стабілізація запуску через Aspire (що було виправлено)
| Проблема | Рішення |
| :--- | :--- |
| AppHost падав: ресурс `CargoOrderingService` (БД) конфліктував з проєктом `cargoorderingservice` | Ресурси БД перейменовано на `*-db` + `connectionName` |
| `Newer version of the Aspire.Hosting.AppHost package is required` | SDK вирівняно до 13.6.1 |
| Порожні бази (`Invalid object name ...`) | `MigrateAsync` / `EnsureCreatedAsync` при старті |
| Неможливо створити водія (FK `companies`) / тіньовий водій отримував чужий Id | Виправлено зв'язки «один-до-одного» зі спільним PK + нові міграції |
| gRPC-сервер завжди падав (`grpc.status_code = 2`) | `DriverRpcService` інжектує `IDriverRepository` замість конкретного класу |
| Логи Craftsman-сервісів не потрапляли в Structured Logs | Serilog `WriteTo.Providers` |
| Агрегатор/BFF чекали 30 с і падали | Один конвеєр Polly на клієнт, Service Discovery, внутрішні `api/internal/*` ендпоінти |
| Авторизація без Keycloak блокувала всі виклики | `[Authorize]` / HeimGuard / JwtBearer закоментовано з маркером `[AUTH-DISABLED]` |

#### 3. Верифікація результатів тестування
Після міграції виконано запуск усіх тестових наборів:
- `CargoOrderingService.UnitTests`: **37 Passed** (0 Failed)
- `DriversBlogManagement.UnitTests`: **49 Passed** (0 Failed)
- `DriversManagement.UnitTests`: **57 Passed** (0 Failed)
- `CargoDelivery.HttpAggregator.Tests`: **6 Passed** (3 юніт + 3 функціональні, 0 Failed)
- **Підсумок:** **149 тестів з 149 проходять успішно (100% Green)**.

[⬆ До змісту](#toc) • [Наступний слайд ➡](#slide-15)

---

<a id="slide-15"></a>
### 🏁 Слайд 15. Чек-лист та практична інструкція запуску для новачка ("Як для чайників")

#### 1. Передумови (Prerequisites)
Переконайтеся, що на вашому комп'ютері встановлено:
1. **.NET 10 SDK** (перевірте командою `dotnet --version`).
2. **Docker Desktop** (переконайтеся, що він запущений, оскільки Aspire створюватиме контейнери RabbitMQ та SQL Server).

---

#### 2. Запуск всієї системи однією командою
> 📍 **Шлях до файлу оркестратора:** `02-Microservices-Craftsman/Aspire/CargoDelivery.AppHost/CargoDelivery.AppHost.csproj`

[👉 **Перейти до файлу (клікніть тут)**](../Aspire/CargoDelivery.AppHost/CargoDelivery.AppHost.csproj)

Відкрийте термінал у корені репозиторію та виконайте:

```powershell
dotnet run --project "02-Microservices-Craftsman\Aspire\CargoDelivery.AppHost\CargoDelivery.AppHost.csproj"
```

*Або у Visual Studio / Rider: виберіть `CargoDelivery.AppHost` як проєкт за замовчуванням (Set as Startup Project) і натисніть `F5`.*

---

#### 3. Що відбудеться автоматично:
1. Aspire відкриє у вашому браузері **Aspire Dashboard**.
2. Docker завантажить і запустить:
   - Контейнер `rabbitmq` з веб-панеллю на порту `15672`.
   - Контейнер `sqlserver` з трьома ізольованими базами даних.
3. Дочекається повної готовності баз та черг (`.WaitFor()`).
4. Запустить усі 4 Web API (порти фіксовані в `launchSettings.json` кожного сервісу):
   - `driversmanagement` — `https://localhost:5001/swagger`
   - `driversblogmanagement` — `https://localhost:5002/swagger`
   - `cargoorderingservice` — `https://localhost:5003/swagger`
   - `httpaggregator` — `https://localhost:57390/swagger`
5. Автоматично наповнить бази демо-даними (Bogus): 50 водіїв → події RabbitMQ → тіньові копії → 50 постів і 50 замовлень із доставками. Візьміть `id` будь-якого водія з `GET https://localhost:5001/api/drivers/all`.

---

#### 4. Як провести повноцінну демонстрацію (Showcase Demo):
Найзручніше — відкрити [CargoDelivery.HttpAggregator.http](../CargoDelivery.HttpAggregator/CargoDelivery.HttpAggregator.http) і виконувати запити по черзі. Або через Swagger:
1. **Відкрийте Swagger:** У дашборді на вкладці *Resources* клікніть на посилання `driversmanagement` та перейдіть у Swagger UI.
2. **Асинхронна шина подій (MassTransit + RabbitMQ):**
   Виконайте `POST /api/drivers` для створення нового водія (авторизацію вимкнено — токен не потрібен).
   *Подія `IDriverCreated` миттєво відправляється у RabbitMQ Fanout Exchange та реплікується в бази даних блогу і замовлень. Перевірка: `GET https://localhost:5002/api/drivers`.*
3. **Демо-дані:** `POST https://localhost:5003/api/internal/deliveries/demo-seed?driverId={id}` і `POST https://localhost:5002/api/internal/postaboutdrivers/demo-seed?driverId={id}`.
4. **Паралельний HTTP Aggregator / BFF (Task.WhenAll):**
   Виконайте `GET /api/v1/driver-dashboard/{id}` (BFF у DriversManagement) або `GET https://localhost:57390/api/v1/shipment-summary/{id}` (виділений агрегатор).
   *Агрегатор паралельно опитає `CargoOrderingService` та `DriversBlogManagement`, зливаючи інформацію у єдиний об'єднаний DTO.*
5. **Синхронний бінарний RPC (gRPC HTTP/2):**
   Виконайте `GET /api/drivers/{id}`.
   *Сервіс виконає прямий бінарний виклик `GetDriverWithPosts` до віддаленого сервера блогу з автоматичним Graceful Degradation fallback при аварії — у відповіді поле `posts`.*
6. **Подивіться магію розподіленої телеметрії у Traces:**  
   Перейдіть у вкладку *Traces* у дашборді:  
   *Ви побачите всі три типи взаємодії (HTTP REST, бінарний gRPC та асинхронний RabbitMQ) у єдиному дереві спанів з точним таймінгом кожного виклику та SQL-запитів EF Core!*

[⬆ До змісту](#toc)
