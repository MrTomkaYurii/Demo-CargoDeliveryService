# 🎯 01. Фундаментальна Оркестрація Розподілених Систем: .NET 10 Aspire AppHost Deep Dive

> **Розділ:** `02-Microservices-Craftsman` • **Модуль:** `02-01-Aspire-Documentation`  
> **Роль матеріалу:** Поглиблений академічно-інженерний посібник з архітектури оркестрації, топології та життєвого циклу розподілених вузлів.  
> **Стек:** .NET 10, C# 13, .NET Aspire 13, `Aspire.Hosting.RabbitMQ`, `Aspire.Hosting.SqlServer`, Docker Desktop / Podman.  
> **Офіційна специфікація Microsoft:** [.NET Aspire AppHost and Orchestration Deep Dive](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host)

---

## 📑 Зміст Архітектурного Модуля

- [1. Теоретичний базис: Омана розподілених обчислень та еволюція інфраструктури](#section-1)
- [2. Порівняльний аналіз: Чому Docker Compose вичерпав себе в Enterprise-системах](#section-2)
- [3. Архітектурна топологія системи: Повний граф вузлів та зв'язків](#section-3)
- [4. Анатомія проєкту CargoDelivery.AppHost: Деконструкція Program.cs / AppHost.cs](#section-4)
- [5. Керування контейнеризованими ресурсами: RabbitMQ 3.13 та MS SQL Server 2022](#section-5)
- [6. Математика узгодженості старту: Патерн .WaitFor() та Directed Acyclic Graph (DAG)](#section-6)
- [7. Інтеграція виділеного диспетчерського хабу CargoDelivery.HttpAggregator](#section-7)
- [8. Механізм Dynamic Configuration Injection: Connection Strings та Service Discovery](#section-8)
- [9. Покроковий практичний чек-лист запуску та діагностики для інженера](#section-9)

---

<a id="section-1"></a>
## 1. Теоретичний базис: Омана розподілених обчислень та еволюція інфраструктури

Під час переходу від монолітного ядра до розподіленої мікросервісної архітектури інженери стикаються з фундаментальними викликами, сформульованими Пітером Дойчем (Peter Deutsch) як **«8 Оман розподілених обчислень» (The 8 Fallacies of Distributed Computing)**:

1. **Мережа надійна (The network is reliable):** У розподіленій системі пакети губляться, сокети обриваються, маршрутизатори перезавантажуються.
2. **Затримка дорівнює нулю (Latency is zero):** Міжпроцесна взаємодія через пам'ять (In-Memory call) займає наносекунди ($\approx 10-50\text{ ns}$), тоді як мережевий HTTP/TCP виклик у локальному кластері займає одиниці мілісекунд ($\approx 2-15\text{ ms}$) — тобто в сотні тисяч разів повільніше.
3. **Пропускна здатність нескінченна (Bandwidth is infinite):** Канали зв'язку мають фізичні ліміти, а серіалізація/десеріалізація JSON навантажує CPU.
4. **Мережа безпечна (The network is secure):** Внутрішній контур вимагає автентифікації (mTLS, JWT, Secret Management).
5. **Топологія не змінюється (Topology doesn't change):** Контейнери масштабуються, перезапускаються та отримують динамічні IP-адреси.
6. **Існує один системний адміністратор (There is one administrator):** Різні сервіси мають різні версії, залежності та вимоги до оточення.
7. **Вартість транспортування дорівнює нулю (Transport cost is zero):** Маршрутизація, проксі-шари та серіалізація споживають відчутні обчислювальні ресурси.
8. **Мережа однорідна (The network is homogeneous):** Вузли можуть працювати на різних ОС, протоколах та версіях рантаймів.

```mermaid
flowchart TD
    subgraph Monolith ["🏛️ Монолітна архітектура (In-Memory Call)"]
        M_UI["UI Контролер"] -->|Call by reference: ~30 ns| M_BLL["Бізнес-логіка"]
        M_BLL -->|In-Process: ~50 ns| M_DAL["Шар доступу до даних"]
    end

    subgraph Distributed ["⚡ Розподілена система (Distributed Network)"]
        D_UI["HttpAggregator"] -->|TCP Handshake + TLS + Serialization: ~5-25 ms| D_DM["DriversManagement"]
        D_UI -->|DNS Resolve + Network Latency: ~10-40 ms| D_CO["CargoOrderingService"]
        D_UI -->|Socket Allocation + HTTP Transfer: ~8-30 ms| D_DB["DriversBlogManagement"]
    end
```

### Наслідок для оркестрації:
Якщо в моноліті всі класи гарантовано завантажуються в пам'ять одночасно завантажувачем класів CLR, то в мікросервісній системі кожен вузол — це окремий незалежний процес операційної системи. Якщо сервіс замовлень спробує надіслати запит до сервісу водіїв до того, як останній підніме свій HTTP-слухач, уся система зазнає каскадного збою (**Cascading Failure**).

---

<a id="section-2"></a>
## 2. Порівняльний аналіз: Чому Docker Compose вичерпав себе в Enterprise-системах

Протягом останнього десятиліття стандартом локального запуску був інструмент **Docker Compose** (`docker-compose.yaml`). Проте на етапі активної розробки складних систем він створює серйозні інженерні перешкоди:

| Критерій | Традиційний підхід (Docker Compose YAML) | .NET 10 Aspire Orchestration (C# AppHost) |
| :--- | :--- | :--- |
| **Парадигма опису** | Декларативний, нетипізований текст YAML | Строго типізований об'єктно-орієнтований C# код |
| **Виявлення помилок (Feedback Loop)** | Лише у Runtime: помилка в назві змінної призводить до `NullReferenceException` | **Compile-Time:** Компілятор Roslyn валідує всі типи, посилання та проєкти до запуску |
| **Рефакторинг коду** | Зміна імені проєкту ламає YAML; пошук і заміна вручну | Повна підтримка IDE (Visual Studio / Rider): F2 перейменовує проєкт скрізь |
| **Управління портами** | Жорсткий хардкод (`ports: "5001:5001"`). Конфлікт портів зупиняє роботу розробника | **Динамічний порт-пул:** Aspire автоматично підбирає вільні порти ОС та прокидає їх |
| **Контроль готовності залежностей** | Примітивний `depends_on`, який перевіряє лише запуск процесу, а не готовність порту | Інтелектуальний `.WaitFor(resource)`, що спирається на Health Probes |
| **Діагностика та налагодження (Debugging)** | Складне підключення віддаленого відладчика через Docker сокети | Звичайний запуск натисканням однієї клавіші **F5** з повноцінним покроковим дебагом |
| **Централізована телеметрія** | Необхідність окремо розгортати та конфігурувати Jaeger, Prometheus, Grafana | Вбудований повнофункціональний **Aspire Dashboard** з підтримкою OTLP "з коробки" |

---

<a id="section-3"></a>
## 3. Архітектурна топологія системи: Повний граф вузлів та зв'язків

В інженерному рішенні `Demo-CargoDeliveryService` модуль оркестрації `CargoDelivery.AppHost` керує шістьма основними ресурсами: 2 контейнерами інфраструктури, 3 ізольованими базами даних та 4 мікросервісними веб-додатками:

```mermaid
flowchart TB
    subgraph Dashboard ["📊 .NET Aspire Dashboard (OTLP 18889)"]
        D_UI["Telemetry Engine: Traces • Logs • Metrics • Health Matrix"]
    end

    subgraph AppHostContainer ["🎯 CargoDelivery.AppHost (Orchestrator Process)"]
        direction TB

        subgraph Infra ["🐳 Docker / Podman Engine (Managed Containers)"]
            RMQ[("🐰 RabbitMQ 3.13-management<br/>Port: 5672 (AMQP) | Port: 15672 (UI)")]
            SQL[("💾 MS SQL Server 2022 Developer<br/>Default Port: 1433")]
            
            DB_DM[("dev_driversmanagement")]
            DB_DB[("dev_driversblogmanagement")]
            DB_CO[("dev_cargoorderingservice")]
            
            SQL --> DB_DM
            SQL --> DB_DB
            SQL --> DB_CO
        end

        subgraph CoreAPIs ["⚡ Business Microservice Nodes (.NET 10)"]
            DM["🚚 DriversManagement<br/>(Core Aggregate Root & gRPC Client)"]
            DB["📝 DriversBlogManagement<br/>(Social Domain & gRPC Server)"]
            CO["📦 CargoOrderingService<br/>(Logistics Delivery & Orders API)"]
        end

        subgraph DispatcherHub ["🛡️ Resilient Gateway Layer"]
            AGG["🌐 CargoDelivery.HttpAggregator<br/>(Logistics Dispatcher Hub • Scatter-Gather)"]
        end
    end

    %% Interservice HTTP Communications
    AGG ==>|1. HTTP GET /api/drivers| DM
    AGG ==>|2. HTTP GET /api/deliveries| CO
    AGG ==>|3. HTTP GET /api/postaboutdrivers| DB

    %% gRPC Direct Point-to-Point
    DM -.->|"gRPC Protobuf (HTTP/2)"| DB

    %% Asynchronous Event Broker
    DM ==>|Publish IDriverCreated| RMQ
    CO ==>|Publish IOrderCreated| RMQ
    RMQ ==>|Consume IDriverCreated| DB
    RMQ ==>|Consume IDriverCreated| CO
    RMQ ==>|Consume IOrderCreated| DM

    %% Database Attachments
    DM --> DB_DM
    DB --> DB_DB
    CO --> DB_CO

    %% Telemetry Exporters
    AppHostContainer -.->|OTLP Protocol gRPC/Protobuf| D_UI
```

---

<a id="section-4"></a>
## 4. Анатомія проєкту CargoDelivery.AppHost: Деконструкція Program.cs / AppHost.cs

> 📍 **Вихідні файли в кодовій базі проєкту:**  
> • [AppHost.cs](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/02-Microservices-Craftsman/Aspire/CargoDelivery.AppHost/AppHost.cs)  
> • [CargoDelivery.AppHost.csproj](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/02-Microservices-Craftsman/Aspire/CargoDelivery.AppHost/CargoDelivery.AppHost.csproj)

### 4.1. Конфігурація файлу проєкту (.csproj)
Файл проєкту використовує спеціалізований SDK `Aspire.AppHost.Sdk/13.0.1`:

```xml
<Project Sdk="Aspire.AppHost.Sdk/13.0.1">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <UserSecretsId>35b821b1-2c90-4e9e-b406-d23105cc07d2</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.RabbitMQ" Version="13.6.1" />
    <PackageReference Include="Aspire.Hosting.SqlServer" Version="13.6.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\CargoOrderingService\src\CargoOrderingService\CargoOrderingService.csproj" />
    <ProjectReference Include="..\..\DriversBlogManagement\src\DriversBlogManagement\DriversBlogManagement.csproj" />
    <ProjectReference Include="..\..\DriversManagement\src\DriversManagement\DriversManagement.csproj" />
    <ProjectReference Include="..\..\CargoDelivery.HttpAggregator\CargoDelivery.HttpAggregator.csproj" />
  </ItemGroup>
</Project>
```

#### Ключові аспекти конфігурації:
1. `Aspire.AppHost.Sdk`: Генератор коду компілятора Roslyn автоматично створює статичний типізований клас `Projects.*`, який містить строго типізовані посилання на кожен підключений проєкт C#.
2. `ProjectReference`: Усі 4 мікросервіси є прямими посиланнями на збірки. Якщо ви випадково зламаєте компіляцію одного з сервісів, AppHost не запуститься і одразу повідомить про помилку.

### 4.2. Порядковий розбір виконуваного коду AppHost.cs
Уся оркестрація системи займає менше 100 рядків чистого, самодокументованого C# коду:

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// ====================================================================================
// 🐰 1. ІНФРАСТРУКТУРА: BROKER RABBITMQ (AMQP 0-9-1)
// ====================================================================================
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// ====================================================================================
// 💾 2. ІНФРАСТРУКТУРА: РЕЛЯЦІЙНА БАЗА ДАНИХ MS SQL SERVER 2022
// ====================================================================================
var sqlserver = builder.AddSqlServer("sqlserver");
var driversDb = sqlserver.AddDatabase("DriversManagement", "dev_driversmanagement");
var blogDb = sqlserver.AddDatabase("DriversBlogManagement", "dev_driversblogmanagement");
var cargoDb = sqlserver.AddDatabase("CargoOrderingService", "dev_cargoorderingservice");

// ====================================================================================
// 📦 3. МІКРОСЕРВІС ЗАМОВЛЕНЬ ТА ДОСТАВОК: CargoOrderingService
// ====================================================================================
var cargoApi = builder.AddProject<Projects.CargoOrderingService>("cargoorderingservice")
    .WithReference(cargoDb)
    .WithReference(rabbitmq)
    .WaitFor(cargoDb)
    .WaitFor(rabbitmq);

// ====================================================================================
// 📝 4. МІКРОСЕРВІС БЛОГУ ВОДІЇВ: DriversBlogManagement
// ====================================================================================
var blogApi = builder.AddProject<Projects.DriversBlogManagement>("driversblogmanagement")
    .WithReference(blogDb)
    .WithReference(rabbitmq)
    .WaitFor(blogDb)
    .WaitFor(rabbitmq);

// ====================================================================================
// 🚚 5. ГОЛОВНИЙ СЕРВІС ВОДІЇВ: DriversManagement
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
// ====================================================================================
var httpAggregator = builder.AddProject<Projects.CargoDelivery_HttpAggregator>("httpaggregator")
    .WithReference(driversApi)
    .WithReference(cargoApi)
    .WithReference(blogApi)
    .WaitFor(driversApi)
    .WaitFor(cargoApi)
    .WaitFor(blogApi);

builder.Build().Run();
```

---

<a id="section-5"></a>
## 5. Керування контейнеризованими ресурсами: RabbitMQ 3.13 та MS SQL Server 2022

### 5.1. Контейнер RabbitMQ
Метод `builder.AddRabbitMQ("rabbitmq").WithManagementPlugin()` виконує такі дії:
1. Завантажує офіційний контейнер `rabbitmq:3-management`.
2. Конфігурує два мережеві порти:
   * **5672 (AMQP):** Для протоколу обміну бінарними фреймами повідомлень бібліотеки MassTransit.
   * **15672 (HTTP):** Веб-панель керування чергами та обмінниками (Exchanges), доступна за прямим посиланням з Aspire Dashboard.
3. Генерує випадковий або фіксований обліковий запис (`guest/guest`) та автоматично формує рядок підключення:
   `amqp://guest:guest@localhost:{random_port}`.

### 5.2. Контейнер MS SQL Server 2022 та Database-per-Microservice
Команда `builder.AddSqlServer("sqlserver")` створює один спільний інстанс СУБД MS SQL Server 2022 Developer Edition, але за допомогою викликів `.AddDatabase(...)` система строго реалізує архітектурний патерн **Database-per-Microservice**:
* `dev_driversmanagement`: Схема досьє водія, фото, техогляди, закріплені вантажівки.
* `dev_driversblogmanagement`: Схема соціального блогу, публікації, лайки, коментарі.
* `dev_cargoorderingservice`: Схема логістичних рейсів, вантажів, статусів доставок.

> 🔒 **Архітектурна перевага:**  
> Кожен мікросервіс отримує рядок підключення **тільки до своєї бази даних**. Прямі крос-базові запити (`SELECT * FROM dev_driversmanagement.dbo.Drivers`) на рівні коду суворо унеможливлені. Будь-який обмін даними здійснюється виключно через контракти API або шину подій!

---

<a id="section-6"></a>
## 6. Математика узгодженості старту: Патерн .WaitFor() та Directed Acyclic Graph (DAG)

У розподіленій системі послідовність ініціалізації вузлів описується математичною структурою **Орієнтованого Ациклічного Графа (DAG — Directed Acyclic Graph)**:

```mermaid
graph TD
    subgraph Layer0 ["Рівень 0: Фізична Інфраструктура (Docker)"]
        SQL["💾 MS SQL Server"]
        RMQ["🐰 RabbitMQ Broker"]
    end

    subgraph Layer1 ["Рівень 1: Логічні Бази Даних"]
        DB1["🗄️ dev_driversmanagement"]
        DB2["🗄️ dev_driversblogmanagement"]
        DB3["🗄️ dev_cargoorderingservice"]
    end

    subgraph Layer2 ["Рівень 2: Автономні Бізнес-Сервіси"]
        CO["📦 CargoOrderingService"]
        DB["📝 DriversBlogManagement"]
    end

    subgraph Layer3 ["Рівень 3: Сервіси зі Зв'язками"]
        DM["🚚 DriversManagement"]
    end

    subgraph Layer4 ["Рівень 4: Агрегатори Верхнього Рівня"]
        AGG["🛡️ CargoDelivery.HttpAggregator"]
    end

    SQL --> DB1 & DB2 & DB3
    DB3 --> CO
    RMQ --> CO
    DB2 --> DB
    RMQ --> DB

    DB1 --> DM
    RMQ --> DM
    CO -.->|WaitFor| DM
    DB -.->|WaitFor| DM

    DM -.->|WaitFor| AGG
    CO -.->|WaitFor| AGG
    DB -.->|WaitFor| AGG
```

### Чому без `.WaitFor()` система падає? (The Race Condition Problem)
Коли Docker створює контейнер SQL Server, процес процесора стартує за $0.8\text{ секунди}$. Але внутрішній механізм бази даних ініціалізує буферний пул (Buffer Pool), створює системні таблиці `master` та відновлює журнал транзакцій ще $10-15\text{ секунд}$.
* **Без `.WaitFor()`:** .NET додаток стартує одночасно, миттєво викликає `DbContext.Database.Migrate()` і отримує фатальний виняток:
  ```text
  Microsoft.Data.SqlClient.SqlException (0x80131904): 
  A network-related or instance-specific error occurred while establishing a connection to SQL Server.
  ```
* **З `.WaitFor(cargoDb)`:** Aspire переводить проєкт `CargoOrderingService` у статус `Waiting for dependencies`. Внутрішній оркестратор періодично пінгує порт 1433 та виконує команду перевірки готовності. Тільки коли SQL Server сигналізує про повну готовність приймати запити, знімається блокування запуску .NET-процесу!

---

<a id="section-7"></a>
## 7. Інтеграція виділеного диспетчерського хабу CargoDelivery.HttpAggregator

Для забезпечення чистої, ненавантаженої зайвими залежностями демонстрації синхронної HTTP-комунікації ми зареєстрували окремий мікросервіс:
> 📍 [CargoDelivery.HttpAggregator.csproj](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/02-Microservices-Craftsman/CargoDelivery.HttpAggregator/CargoDelivery.HttpAggregator.csproj)

В `AppHost.cs` реєстрація має такий вигляд:

```csharp
var httpAggregator = builder.AddProject<Projects.CargoDelivery_HttpAggregator>("httpaggregator")
    .WithReference(driversApi)
    .WithReference(cargoApi)
    .WithReference(blogApi)
    .WaitFor(driversApi)
    .WaitFor(cargoApi)
    .WaitFor(blogApi);
```

### Що відбувається під капотом під час виклику `.WithReference(...)`:
1. **Реєстрація у топологічному графі:** Aspire Dashboard фіксує зв'язки між агрегатором та трьома суміжними API.
2. **Динамічна генерація Service Discovery змінних:**
   У процес `httpaggregator` автоматично інжектуються змінні середовища:
   * `services__driversmanagement__http__0 = http://localhost:{port_dm}`
   * `services__cargoorderingservice__http__0 = http://localhost:{port_co}`
   * `services__driversblogmanagement__http__0 = http://localhost:{port_db}`
3. **Захист життєвого циклу:** Агрегатор не запускається до тих пір, поки всі три мікросервіси не піднімуть власні HTTP-слухачі та успішно не дадуть відповідь на зонд `/health`.

---

<a id="section-8"></a>
## 8. Механізм Dynamic Configuration Injection: Connection Strings та Service Discovery

Aspire усуває людський фактор конфігурації. Таблиця нижче демонструє, як виклики AppHost транслюються у конкретні змінні процесу:

| Код у AppHost.cs | Що інжектується у залежний сервіс (Environment Variable) | Як це зчитує мікросервіс у C# |
| :--- | :--- | :--- |
| `builder.AddRabbitMQ("rabbitmq")` | `ConnectionStrings__rabbitmq` | `builder.Configuration.GetConnectionString("rabbitmq")` |
| `sqlserver.AddDatabase("DriversManagement")` | `ConnectionStrings__DriversManagement` | `builder.Configuration.GetConnectionString("DriversManagement")` |
| `.WithReference(cargoApi)` | `services__cargoorderingservice__http__0` | `builder.Services.AddHttpClient(c => c.BaseAddress = new Uri("http://cargoorderingservice"))` |
| Автоматично для всіх сервісів | `OTEL_EXPORTER_OTLP_ENDPOINT` | Зчитується методом `AddOpenTelemetryExporters()` у `ServiceDefaults` |
| Автоматично для кожного проєкту | `OTEL_SERVICE_NAME` | Встановлює унікальне ім'я вузла в трасуванні (`driversmanagement`, тощо) |

```mermaid
sequenceDiagram
    autonumber
    participant Host as 🎯 Aspire AppHost
    participant Engine as 🐳 Docker Engine
    participant Service as 🚚 DriversManagement (.NET)
    participant Dash as 📊 Aspire Dashboard

    Host->>Engine: Запуск контейнерів SQL Server та RabbitMQ
    Engine-->>Host: Контейнери піднято, порти призначено динамічно
    Note over Host: Формування змінних оточення (Port Mapping)
    Host->>Service: Старт процесу з ConnectionStrings__* та services__*
    Service->>Service: builder.AddServiceDefaults()
    Service->>Dash: OTLP Handshake (Трансляція логів, метрик, трейсів)
    Service-->>Host: Зонд /health повертає 200 OK
    Note over Host: Розблокування наступних вузлів у графі DAG
```

---

<a id="section-9"></a>
## 9. Покроковий практичний чек-лист запуску та діагностики для інженера

### Крок 1. Перевірка вимог оточення
Перед запуском переконайтеся, що на робочій станції запущено Docker-демон:
```powershell
docker info
```
*(У виводі повинен відображатися активний статус контейнерного рушія).*

### Крок 2. Запуск всієї системи однією командою
У терміналі виконайте команду запуску оркестратора:
```powershell
dotnet run --project "02-Microservices-Craftsman\Aspire\CargoDelivery.AppHost\CargoDelivery.AppHost.csproj"
```

### Крок 3. Що спостерігати в інтерфейсі .NET Aspire Dashboard
1. Після запуску автоматично відкриється вкладка браузера з адресою `http://localhost:18888` (або випадковим захищеним портом з токеном).
2. Перейдіть у розділ **Resources**:
   * Перевірте, що всі 6 вузлів системи світяться зеленим індикатором `Running`.
   * Зверніть увагу на колонку **Source**: для сервісів там вказано шлях до `.csproj`, для баз даних — назва Docker-образу.
   * Зверніть увагу на колонку **Endpoints**: кожен мікросервіс має клікабельне посилання, натискання на яке відкриває Swagger або цільовий API.
3. Перейдіть до розділу **Console Logs**:
   * Оберіть ресурс `httpaggregator`.
   * Переконайтеся, що під час старту немає жодного повідомлення `SocketException` чи `Connection Refused`.
