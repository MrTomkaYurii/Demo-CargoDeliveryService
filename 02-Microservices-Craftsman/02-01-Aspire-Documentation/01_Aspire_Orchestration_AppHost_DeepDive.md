# 🎯 01. Фундаментальна Оркестрація Розподілених Систем: .NET 10 Aspire AppHost Deep Dive

> **Розділ:** `02-Microservices-Craftsman` • **Модуль:** `02-01-Aspire-Documentation`  
> **Роль матеріалу:** Поглиблений академічно-інженерний посібник з архітектури оркестрації, топології та життєвого циклу розподілених вузлів.  
> **Стек:** .NET 10, C# 14, .NET Aspire 13.6 (`Aspire.AppHost.Sdk/13.6.1`), `Aspire.Hosting.RabbitMQ`, `Aspire.Hosting.SqlServer`, Docker Desktop / Podman.  
> **Офіційна специфікація Microsoft:** [.NET Aspire AppHost and Orchestration Deep Dive](https://learn.microsoft.com/dotnet/aspire/fundamentals/app-host)

---

## 📑 Зміст Архітектурного Модуля

- [1. Теоретичний базис: Омана розподілених обчислень та еволюція інфраструктури](#section-1)
- [2. Порівняльний аналіз: Чому Docker Compose вичерпав себе в Enterprise-системах](#section-2)
- [3. Архітектурна топологія системи: Повний граф вузлів та зв'язків](#section-3)
- [4. Анатомія проєкту CargoDelivery.AppHost: Деконструкція AppHost.cs](#section-4)
- [5. Керування контейнеризованими ресурсами: RabbitMQ 4.x та MS SQL Server 2022](#section-5)
- [6. Математика узгодженості старту: Патерн .WaitFor() та Directed Acyclic Graph (DAG)](#section-6)
- [7. Інтеграція виділеного диспетчерського хабу CargoDelivery.HttpAggregator](#section-7)
- [8. Механізм Dynamic Configuration Injection: Connection Strings та Service Discovery](#section-8)
- [9. Життєвий цикл ресурсів: DCP, проксі портів, ефемерні контейнери та «старі бази»](#section-9)
- [10. Покроковий практичний чек-лист запуску та діагностики для інженера](#section-10)
- [11. Контрольні запитання та практичні завдання](#section-11)

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

### Як кожна омана проявляється в нашому проєкті

| Омана | Де ми з нею стикаємось | Чим відповідаємо в коді |
| :--- | :--- | :--- |
| Мережа надійна | Блог може впасти посеред запиту агрегатора | Polly Retry + Graceful Degradation ([гайд 06](06_ServiceDiscovery_Resilience_Polly_HttpClient.md), [гайд 03](03_HttpAggregator_Gateway_Architecture_And_Design.md)) |
| Затримка дорівнює нулю | 3 послідовні виклики = сума затримок | Паралельний Fan-Out через `Task.WhenAll` ([гайд 03, §4](03_HttpAggregator_Gateway_Architecture_And_Design.md#section-4)) |
| Топологія не змінюється | Порти контейнерів RabbitMQ/SQL щоразу інші | Логічні імена + Service Discovery ([§8](#section-8)) |
| Мережа безпечна | Сервіси довіряють один одному | Внутрішні `api/internal/*` ендпоінти — окремий контур (у демо без автентифікації, див. [гайд 03](03_HttpAggregator_Gateway_Architecture_And_Design.md)) |

### Наслідок для оркестрації:
Якщо в моноліті всі класи гарантовано завантажуються в пам'ять одночасно завантажувачем класів CLR, то в мікросервісній системі кожен вузол — це окремий незалежний процес операційної системи. Якщо сервіс замовлень спробує надіслати запит до сервісу водіїв до того, як останній підніме свій HTTP-слухач, уся система зазнає каскадного збою (**Cascading Failure**).

Саме тому оркестратор має знати **три речі** про кожен вузол: *що* запустити (проєкт чи контейнер), *з якою конфігурацією* (рядки підключення, адреси сусідів) і *коли* (після яких залежностей). AppHost описує всі три в одному C#-файлі.

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
| **Контроль готовності залежностей** | `depends_on` за замовчуванням чекає лише *старту* контейнера; для готовності треба вручну писати `healthcheck` + `condition: service_healthy` | `.WaitFor(resource)` чекає стану **Healthy**: для контейнерів — вбудовані перевірки інтеграцій, для проєктів — `.WithHttpHealthCheck("/health")` |
| **Діагностика та налагодження (Debugging)** | Складне підключення віддаленого відладчика через Docker сокети | Звичайний запуск натисканням однієї клавіші **F5** з повноцінним покроковим дебагом |
| **Централізована телеметрія** | Необхідність окремо розгортати та конфігурувати Jaeger, Prometheus, Grafana | Вбудований повнофункціональний **Aspire Dashboard** з підтримкою OTLP "з коробки" |

> 💡 **Важливо розуміти межі Aspire.** AppHost — це насамперед інструмент **локальної розробки та опису топології**. У продакшн ця модель «публікується» (наприклад, `aspire publish` / `azd`) у маніфести Kubernetes, Docker Compose або Azure Container Apps. Тобто Aspire не замінює Kubernetes, а генерує для нього опис.

---

<a id="section-3"></a>
## 3. Архітектурна топологія системи: Повний граф вузлів та зв'язків

В інженерному рішенні `Demo-CargoDeliveryService` модуль оркестрації `CargoDelivery.AppHost` керує **9 ресурсами**: 2 контейнерами інфраструктури (`rabbitmq`, `sqlserver`), 3 логічними базами даних усередині SQL Server (`*-db`) та 4 мікросервісними веб-додатками.

```mermaid
flowchart TB
    subgraph Dashboard ["📊 .NET Aspire Dashboard (https://localhost:17048, OTLP https://localhost:21114)"]
        D_UI["Telemetry Engine: Traces • Logs • Metrics • Health Matrix"]
    end

    subgraph AppHostContainer ["🎯 CargoDelivery.AppHost (Orchestrator Process)"]
        direction TB

        subgraph Infra ["🐳 Docker / Podman Engine (Managed Containers)"]
            RMQ[("🐰 RabbitMQ 4.x-management<br/>AMQP + Management UI (динамічні порти)")]
            SQL[("💾 MS SQL Server 2022<br/>(динамічний порт)")]
            
            DB_DM[("driversmanagement-db<br/>dev_driversmanagement")]
            DB_DB[("driversblogmanagement-db<br/>dev_driversblogmanagement")]
            DB_CO[("cargoorderingservice-db<br/>dev_cargoorderingservice")]
            
            SQL --> DB_DM
            SQL --> DB_DB
            SQL --> DB_CO
        end

        subgraph CoreAPIs ["⚡ Business Microservice Nodes (.NET 10)"]
            DM["🚚 DriversManagement :5001<br/>(Core Aggregate Root, BFF & gRPC Client)"]
            DB["📝 DriversBlogManagement :5002<br/>(Social Domain & gRPC Server)"]
            CO["📦 CargoOrderingService :5003<br/>(Logistics Delivery & Orders API)"]
        end

        subgraph DispatcherHub ["🛡️ Resilient Gateway Layer"]
            AGG["🌐 CargoDelivery.HttpAggregator :57390<br/>(Logistics Dispatcher Hub • Scatter-Gather)"]
        end
    end

    %% Interservice HTTP Communications
    AGG ==>|1. HTTP GET /api/internal/drivers/id| DM
    AGG ==>|2. HTTP GET /api/internal/deliveries| CO
    AGG ==>|3. HTTP GET /api/internal/postaboutdrivers| DB

    %% gRPC Direct Point-to-Point
    DM -.->|"gRPC Protobuf (HTTP/2)"| DB

    %% Asynchronous Event Broker
    DM ==>|Publish IDriverCreated / IDriverUpdated| RMQ
    CO ==>|Publish IOrderCreated| RMQ
    RMQ ==>|Consume IDriverCreated / IDriverUpdated| DB
    RMQ ==>|Consume IDriverCreated / IDriverUpdated| CO

    %% Database Attachments
    DM --> DB_DM
    DB --> DB_DB
    CO --> DB_CO

    %% Telemetry Exporters
    AppHostContainer -.->|OTLP Protocol gRPC/Protobuf| D_UI
```

> ℹ️ Подію `IOrderCreated` CargoOrderingService публікує в обмінник `order-created`, але **жоден сервіс на неї поки не підписаний** — це «заділ» під майбутні модулі (Billing, Notifications). RabbitMQ у такому разі просто відкидає повідомлення, бо до обмінника не прив'язано жодної черги. Детальніше — у [04_Patterns_Asynchronous_Messaging](../02-02-Communication-Documentation/04_Patterns_Asynchronous_Messaging_MassTransit_RabbitMQ.md).

---

<a id="section-4"></a>
## 4. Анатомія проєкту CargoDelivery.AppHost: Деконструкція AppHost.cs

> 📍 **Вихідні файли в кодовій базі проєкту:**  
> • [AppHost.cs](../Aspire/CargoDelivery.AppHost/AppHost.cs)  
> • [CargoDelivery.AppHost.csproj](../Aspire/CargoDelivery.AppHost/CargoDelivery.AppHost.csproj)  
> • [Properties/launchSettings.json](../Aspire/CargoDelivery.AppHost/Properties/launchSettings.json) — адреси дашборду та OTLP-приймача

### 4.1. Конфігурація файлу проєкту (.csproj)
Файл проєкту використовує спеціалізований SDK `Aspire.AppHost.Sdk/13.6.1` ([csproj#L1](../Aspire/CargoDelivery.AppHost/CargoDelivery.AppHost.csproj#L1)):

```xml
<Project Sdk="Aspire.AppHost.Sdk/13.6.1">
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
1. `Aspire.AppHost.Sdk`: Генератор коду компілятора Roslyn автоматично створює статичний типізований клас `Projects.*`, який містить строго типізовані посилання на кожен підключений проєкт C#. Крапки в назві проєкту замінюються на підкреслення: `CargoDelivery.HttpAggregator` → `Projects.CargoDelivery_HttpAggregator`.
2. `ProjectReference`: Усі 4 мікросервіси є прямими посиланнями на збірки. Якщо ви випадково зламаєте компіляцію одного з сервісів, AppHost не запуститься і одразу повідомить про помилку.
3. **Версія SDK = версія пакетів хостингу.** SDK приносить разом із собою оркестратор **DCP** (Developer Control Plane) і дашборд. Якщо SDK старіший за пакети `Aspire.Hosting.*`, старт падає з помилкою `Newer version of the Aspire.Hosting.AppHost package is required` — саме це сталося в проєкті, коли SDK був `13.0.1`, а пакети — `13.6.1`. Тримайте їх синхронними.
4. `UserSecretsId`: сюди Aspire зберігає згенеровані секрети ресурсів (пароль SQL `sa`, пароль RabbitMQ), щоб вони не змінювались між запусками.

### 4.2. Порядковий розбір виконуваного коду AppHost.cs
Уся оркестрація системи займає близько 30 рядків C# коду (решта — пояснювальні коментарі українською):

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// 🐰 1. Брокер RabbitMQ (AMQP 0-9-1) + веб-панель
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

// 💾 2. SQL Server 2022 + три ізольовані бази (Database-per-Microservice)
var sqlserver = builder.AddSqlServer("sqlserver");
var driversDb = sqlserver.AddDatabase("driversmanagement-db", "dev_driversmanagement");
var blogDb = sqlserver.AddDatabase("driversblogmanagement-db", "dev_driversblogmanagement");
var cargoDb = sqlserver.AddDatabase("cargoorderingservice-db", "dev_cargoorderingservice");

// 📦 3. CargoOrderingService
var cargoApi = builder.AddProject<Projects.CargoOrderingService>("cargoorderingservice")
    .WithReference(cargoDb, connectionName: "CargoOrderingService")
    .WithReference(rabbitmq)
    .WithHttpHealthCheck("/health")
    .WaitFor(cargoDb)
    .WaitFor(rabbitmq);

// 📝 4. DriversBlogManagement
var blogApi = builder.AddProject<Projects.DriversBlogManagement>("driversblogmanagement")
    .WithReference(blogDb, connectionName: "DriversBlogManagement")
    .WithReference(rabbitmq)
    .WithHttpHealthCheck("/health")
    .WaitFor(blogDb)
    .WaitFor(rabbitmq);

// 🚚 5. DriversManagement (BFF + gRPC-клієнт + продюсер подій)
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

// 🛡️ 6. CargoDelivery.HttpAggregator
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

#### Карта коду: куди дивитися в AppHost.cs

| Блок | Рядки | Що робить |
| :--- | :--- | :--- |
| Створення білдера | [#L13](../Aspire/CargoDelivery.AppHost/AppHost.cs#L13) | `DistributedApplication.CreateBuilder(args)` — модель застосунку (Application Model), до якої додаються ресурси |
| RabbitMQ | [#L27-L28](../Aspire/CargoDelivery.AppHost/AppHost.cs#L27-L28) | Контейнер брокера + Management UI |
| SQL Server та бази | [#L47-L50](../Aspire/CargoDelivery.AppHost/AppHost.cs#L47-L50) | Один контейнер, три логічні БД з унікальними іменами `*-db` |
| CargoOrderingService | [#L60-L68](../Aspire/CargoDelivery.AppHost/AppHost.cs#L60-L68) | Залежить лише від своєї БД і брокера |
| DriversBlogManagement | [#L78-L83](../Aspire/CargoDelivery.AppHost/AppHost.cs#L78-L83) | Залежить лише від своєї БД і брокера |
| DriversManagement | [#L96-L105](../Aspire/CargoDelivery.AppHost/AppHost.cs#L96-L105) | Додатково чекає на блог (gRPC) і замовлення (HTTP BFF) |
| HttpAggregator | [#L116-L123](../Aspire/CargoDelivery.AppHost/AppHost.cs#L116-L123) | Чекає на всі три бізнес-сервіси |
| Запуск | [#L125](../Aspire/CargoDelivery.AppHost/AppHost.cs#L125) | `Build().Run()` — старт DCP, дашборду і всіх ресурсів |

#### Чотири «дієслова» моделі застосунку Aspire

| Метод | Питання, на яке відповідає | Що відбувається фізично |
| :--- | :--- | :--- |
| `Add*` (`AddProject`, `AddRabbitMQ`, `AddSqlServer`, `AddDatabase`) | **Що** запускати? | Створюється ресурс у графі моделі (проєкт, контейнер, дочірній ресурс) |
| `.WithReference(x)` | **Що** знає про сусіда? | У процес інжектуються змінні середовища: рядок підключення або адреси ендпоінтів `x` |
| `.WithHttpHealthCheck(path)` | **Як** зрозуміти, що ресурс готовий? | Aspire періодично викликає `GET path`; 200 → `Healthy` |
| `.WaitFor(x)` | **Коли** запускати? | Старт ресурсу відкладається, доки `x` не стане `Healthy` |

---

<a id="section-5"></a>
## 5. Керування контейнеризованими ресурсами: RabbitMQ 4.x та MS SQL Server 2022

### 5.1. Контейнер RabbitMQ
Метод `builder.AddRabbitMQ("rabbitmq").WithManagementPlugin()` ([AppHost.cs#L27-L28](../Aspire/CargoDelivery.AppHost/AppHost.cs#L27-L28)) виконує такі дії:
1. Завантажує офіційний образ RabbitMQ з плагіном керування — у поточній версії Aspire це `docker.io/library/rabbitmq:4.x-management` (точний тег видно в колонці **Source** дашборду).
2. Описує два ендпоінти контейнера:
   * **AMQP (у контейнері порт 5672):** для протоколу обміну бінарними фреймами повідомлень бібліотеки MassTransit.
   * **Management UI (у контейнері порт 15672):** веб-панель керування чергами та обмінниками (Exchanges), доступна за прямим посиланням з Aspire Dashboard.
   
   На хості обидва ендпоінти отримують **динамічні** порти — у кожному запуску інші. Саме тому сервіси не повинні «знати» порт брокера заздалегідь.
3. Облікові дані: користувач `guest`, а **пароль Aspire генерує** і зберігає як параметр `rabbitmq-password` (вкладка **Parameters** у дашборді; значення зберігається в user secrets AppHost). Залежні сервіси отримують готовий рядок підключення:
   `amqp://guest:{згенерований_пароль}@localhost:{динамічний_порт}`.

> 🔎 **Як потрапити в панель RabbitMQ:** у дашборді в рядку `rabbitmq` клікніть посилання з колонки *URLs* (management-ендпоінт), логін `guest`, пароль — у вкладці *Parameters* (кнопка «показати» біля `rabbitmq-password`). Там видно обмінники `driver-created`, `driver-updated`, `order-created` і кворум-черги `sync-updated-driver`, `ordering-sync-driver`.

### 5.2. Контейнер MS SQL Server 2022 та Database-per-Microservice
Команда `builder.AddSqlServer("sqlserver")` ([AppHost.cs#L47](../Aspire/CargoDelivery.AppHost/AppHost.cs#L47)) створює один спільний інстанс СУБД MS SQL Server 2022 (`mcr.microsoft.com/mssql/server:2022-latest`) і генерує пароль `sa` (параметр `sqlserver-password`). За допомогою викликів `.AddDatabase(...)` ([#L48-L50](../Aspire/CargoDelivery.AppHost/AppHost.cs#L48-L50)) система строго реалізує архітектурний патерн **Database-per-Microservice**:
* `dev_driversmanagement`: Схема досьє водія, фото, техогляди, закріплені вантажівки.
* `dev_driversblogmanagement`: Схема соціального блогу, публікації, лайки, коментарі, тіньові водії.
* `dev_cargoorderingservice`: Схема логістичних рейсів, вантажів, статусів доставок, тіньові водії.

У `AddDatabase(name, databaseName)` два різні імена:
* `name` (`driversmanagement-db`) — **ім'я ресурсу Aspire**: відображається в дашборді, має бути унікальним у межах усього AppHost **без урахування регістру**;
* `databaseName` (`dev_driversmanagement`) — **фізична назва бази** всередині SQL Server. Якщо бази ще немає, Aspire створює її при першому старті.

> 🔒 **Архітектурна перевага:**  
> Кожен мікросервіс отримує рядок підключення **тільки до своєї бази даних**. Прямі крос-базові запити (`SELECT * FROM dev_driversmanagement.dbo.Drivers`) на рівні коду суворо унеможливлені. Будь-який обмін даними здійснюється виключно через контракти API або шину подій!

> ⚖️ **Чесна примітка про «один контейнер на всі бази».** Фізично три бази живуть в одному інстансі SQL Server — це компроміс для локальної розробки (економія RAM). Логічна ізоляція збережена (окремі БД і рядки підключення), але «шумний сусід» може вплинути на продуктивність інших. У продакшні бази зазвичай розносять по окремих серверах або керованих інстансах.

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
        DB1["🗄️ driversmanagement-db"]
        DB2["🗄️ driversblogmanagement-db"]
        DB3["🗄️ cargoorderingservice-db"]
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

### 6.1. Теорія: топологічне сортування
Порядок старту — це **топологічне сортування** графа залежностей: вершину можна запустити лише тоді, коли запущені всі вершини, з яких у неї ведуть ребра. Класичний алгоритм Кана (Kahn, 1962) працює так:
1. Знайти всі вершини без вхідних ребер (у нас — `sqlserver`, `rabbitmq`) і запустити їх **паралельно**.
2. «Видалити» їх із графа; вершини, у яких зникли всі вхідні ребра, стають наступною хвилею.
3. Повторювати, доки граф не спорожніє.

Звідси дві важливі властивості:
* **Паралелізм рівнів.** Сервіси одного рівня (CargoOrderingService і DriversBlogManagement) стартують одночасно — це видно в дашборді за однаковим `Start time`. Загальний час старту ≈ сума часу «найповільнішого» вузла на кожному рівні, а не сума всіх вузлів.
* **Заборона циклів.** Якщо A чекає на B, а B на A, жоден не стартує ніколи (deadlock). Тому граф має бути ациклічним — саме через це агрегатор не може зробити `.WithReference(httpAggregator)` сам на себе.

### 6.2. Що саме означає «готовий» для `.WaitFor()`
Aspire відстежує для кожного ресурсу **стан** (`Starting` → `Running` → …) і **стан здоров'я** (`Healthy` / `Unhealthy`). `.WaitFor(x)` чекає, поки `x` буде `Running` **і** `Healthy`:

| Тип ресурсу | Звідки береться Healthy | У нашому проєкті |
| :--- | :--- | :--- |
| Контейнер SQL Server | Вбудована перевірка інтеграції `Aspire.Hosting.SqlServer` (підключення до сервера) | Автоматично |
| База `*-db` | Перевірка інтеграції: база створена й доступна | Автоматично |
| Контейнер RabbitMQ | Вбудована перевірка інтеграції (AMQP-з'єднання) | Автоматично |
| Проєкт (.NET-сервіс) | **Тільки якщо явно додано** `.WithHttpHealthCheck("/health")`. Без цього проєкт вважається «здоровим», щойно процес запустився — навіть якщо Kestrel ще не слухає порт і міграції не завершені! | Додано для всіх 4 сервісів ([#L66](../Aspire/CargoDelivery.AppHost/AppHost.cs#L66), [#L81](../Aspire/CargoDelivery.AppHost/AppHost.cs#L81), [#L101](../Aspire/CargoDelivery.AppHost/AppHost.cs#L101), [#L120](../Aspire/CargoDelivery.AppHost/AppHost.cs#L120)) |

Ендпоінт `/health`, який опитує Aspire, мапиться в кожному сервісі методом `app.MapDefaultEndpoints()` зі спільного проєкту — [Extensions.cs#L248](../Aspire/CargoDelivery.ServiceDefaults/Extensions.cs#L248). У дашборді він видно як health check з іменем на кшталт `httpaggregator_https_/health_200_check`.

### Чому без `.WaitFor()` система падає? (The Race Condition Problem)
Коли Docker створює контейнер SQL Server, процес у контейнері стартує за $\approx 1\text{ секунду}$. Але внутрішній механізм бази даних ініціалізує буферний пул (Buffer Pool), створює системні таблиці `master` та відновлює журнал транзакцій ще $10-15\text{ секунд}$.
* **Без `.WaitFor()`:** .NET додаток стартує одночасно, миттєво викликає `DbContext.Database.MigrateAsync()` (див. [MigrationHostedService.cs#L48](../DriversManagement/src/DriversManagement/Databases/MigrationHostedService.cs#L48)) і отримує фатальний виняток:
  ```text
  Microsoft.Data.SqlClient.SqlException (0x80131904): 
  A network-related or instance-specific error occurred while establishing a connection to SQL Server.
  ```
* **З `.WaitFor(cargoDb)`:** Aspire переводить проєкт `CargoOrderingService` у стан `Waiting`. Оркестратор періодично виконує перевірку здоров'я ресурсу бази даних. Тільки коли SQL Server і сама база готові приймати запити, знімається блокування запуску .NET-процесу!

---

<a id="section-7"></a>
## 7. Інтеграція виділеного диспетчерського хабу CargoDelivery.HttpAggregator

Для забезпечення чистої, ненавантаженої зайвими залежностями демонстрації синхронної HTTP-комунікації ми зареєстрували окремий мікросервіс:
> 📍 [CargoDelivery.HttpAggregator.csproj](../CargoDelivery.HttpAggregator/CargoDelivery.HttpAggregator.csproj)

В `AppHost.cs` реєстрація має такий вигляд ([#L116-L123](../Aspire/CargoDelivery.AppHost/AppHost.cs#L116-L123)):

```csharp
var httpAggregator = builder.AddProject<Projects.CargoDelivery_HttpAggregator>("httpaggregator")
    .WithReference(driversApi)
    .WithReference(cargoApi)
    .WithReference(blogApi)
    .WithHttpHealthCheck("/health")
    .WaitFor(driversApi)
    .WaitFor(cargoApi)
    .WaitFor(blogApi);
```

### Що відбувається під капотом під час виклику `.WithReference(...)`:
1. **Реєстрація у топологічному графі:** Aspire Dashboard фіксує зв'язки між агрегатором та трьома суміжними API (вкладка **Graph** і секція *References* у деталях ресурсу).
2. **Динамічна генерація Service Discovery змінних:**
   У процес `httpaggregator` автоматично інжектуються змінні середовища (перевірено в деталях ресурсу в дашборді):
   * `services__driversmanagement__https__0 = https://localhost:5001`
   * `services__cargoorderingservice__https__0 = https://localhost:5003`
   * `services__driversblogmanagement__https__0 = https://localhost:5002`
   
   Ключі `http__0` не з'являються, бо в `launchSettings.json` наших сервісів оголошено лише HTTPS-адреси. Тому клієнти агрегатора використовують схему `https+http://` — «спершу HTTPS, якщо немає — HTTP» ([Program.cs#L45](../CargoDelivery.HttpAggregator/Program.cs#L45)).
3. **Захист життєвого циклу:** Агрегатор не запускається до тих пір, поки всі три мікросервіси не піднімуть власні HTTP-слухачі та не дадуть відповідь `200 OK` на зонд `/health` (завдяки `.WithHttpHealthCheck` на кожному з них).

---

<a id="section-8"></a>
## 8. Механізм Dynamic Configuration Injection: Connection Strings та Service Discovery

Aspire усуває людський фактор конфігурації. Таблиця нижче демонструє, як виклики AppHost транслюються у конкретні змінні процесу:

| Код у AppHost.cs | Що інжектується у залежний сервіс (Environment Variable) | Як це зчитує мікросервіс у C# |
| :--- | :--- | :--- |
| `.WithReference(rabbitmq)` | `ConnectionStrings__rabbitmq = amqp://guest:…@localhost:{порт}` | `configuration.GetConnectionString("rabbitmq")` — [MassTransitServiceExtension.cs#L36](../DriversManagement/src/DriversManagement/Extensions/Services/MassTransitServiceExtension.cs#L36) |
| `.WithReference(driversDb, connectionName: "DriversManagement")` | `ConnectionStrings__DriversManagement = Server=…;Database=dev_driversmanagement;…` | `ConnectionStringOptions.DriversManagement` — [ConnectionStringOptions.cs](../DriversManagement/src/DriversManagement/Configurations/ConnectionStringOptions.cs) |
| `.WithReference(cargoApi)` | `services__cargoorderingservice__https__0 = https://localhost:5003` | `new Uri("https+http://cargoorderingservice")` + `AddServiceDiscovery()` — [Program.cs#L48](../CargoDelivery.HttpAggregator/Program.cs#L48) |
| Автоматично для всіх проєктів | `OTEL_EXPORTER_OTLP_ENDPOINT`, `OTEL_EXPORTER_OTLP_HEADERS`, `OTEL_EXPORTER_OTLP_PROTOCOL` | `AddOpenTelemetryExporters()` у ServiceDefaults — [Extensions.cs#L185](../Aspire/CargoDelivery.ServiceDefaults/Extensions.cs#L185) |
| Автоматично для кожного проєкту | `OTEL_SERVICE_NAME`, `OTEL_RESOURCE_ATTRIBUTES` | Встановлює унікальне ім'я вузла в трасуванні (`driversmanagement` тощо) |
| Автоматично для кожного проєкту | `ASPNETCORE_URLS`, `ASPNETCORE_ENVIRONMENT=Development` | Kestrel слухає призначені порти; ендпоінти `/health` і `/alive` відкриті лише в Development |

> 🧠 **Чому без `connectionName` нічого не працювало б?** За замовчуванням ключ рядка підключення дорівнює **імені ресурсу**: `.WithReference(driversDb)` дав би `ConnectionStrings__driversmanagement-db`. Сервіси ж Craftsman читають ключі `DriversManagement`, `DriversBlogManagement`, `CargoOrderingService`. Параметр `connectionName` розв'язує ці дві речі: ім'я ресурсу лишається унікальним для Aspire, а ключ конфігурації — таким, як очікує сервіс.

> 🔁 **Пріоритет джерел конфігурації в ASP.NET Core:** змінні середовища **перекривають** `appsettings.json` і `appsettings.Development.json`. Тому, коли сервіс запущено через AppHost, «старі» рядки підключення з appsettings (наприклад, `localhost,1434`) ігноруються — працює контейнер Aspire. А при автономному запуску (`dotnet run` без AppHost) спрацьовує саме appsettings.

```mermaid
sequenceDiagram
    autonumber
    participant Host as 🎯 Aspire AppHost
    participant Engine as 🐳 Docker Engine
    participant Service as 🚚 DriversManagement (.NET)
    participant Dash as 📊 Aspire Dashboard

    Host->>Engine: Запуск контейнерів SQL Server та RabbitMQ
    Engine-->>Host: Контейнери піднято, порти призначено динамічно
    Host->>Host: Перевірки здоров'я sqlserver / *-db / rabbitmq → Healthy
    Note over Host: Формування змінних оточення (ConnectionStrings__*, services__*, OTEL_*)
    Host->>Service: Старт процесу зі змінними середовища
    Service->>Service: builder.AddServiceDefaults() + міграції БД
    Service->>Dash: OTLP-експорт логів, метрик, трейсів
    Host->>Service: GET /health (WithHttpHealthCheck)
    Service-->>Host: 200 OK → Healthy
    Note over Host: Розблокування наступних вузлів у графі DAG (httpaggregator)
```

---

<a id="section-9"></a>
## 9. Життєвий цикл ресурсів: DCP, проксі портів, ефемерні контейнери та «старі бази»

### 9.1. Хто насправді запускає процеси і контейнери
AppHost не запускає Docker напряму. Він передає опис моделі в **DCP (Developer Control Plane)** — окремий процес `dcp.exe` з Aspire SDK, схожий на «мініатюрний Kubernetes» для розробника:
* DCP створює контейнери через Docker/Podman і запускає .NET-проєкти як звичайні процеси, тому їх можна дебажити як завжди.
* Для кожного ендпоінта проєкту DCP ставить **проксі**: зовнішній порт (наприклад, `5001` з launchSettings) приймає з'єднання і пересилає їх у реальний процес. Тому при зупинці сервісу через дашборд порт **продовжує приймати TCP-з'єднання**, але відповіді не буде — клієнт бачить не миттєву помилку `Connection refused`, а зависання до таймауту. Це важливо для налаштування Polly ([гайд 06](06_ServiceDiscovery_Resilience_Polly_HttpClient.md)).

### 9.2. Контейнери ефемерні — і це рішення проблеми «старих баз»
У нашому AppHost контейнери оголошено **без томів даних** (`WithDataVolume()` не викликається). Отже:
* при кожному запуску AppHost створюється **новий** контейнер SQL Server з порожніми базами;
* при зупинці AppHost контейнери видаляються разом із даними.

Звідси два наслідки, які пояснюють «дивну» поведінку з базами:
1. **Схему треба створювати автоматично при кожному старті.** Це робить `MigrationHostedService` у кожному сервісі ([DriversManagement](../DriversManagement/src/DriversManagement/Databases/MigrationHostedService.cs#L48), [DriversBlogManagement](../DriversBlogManagement/src/DriversBlogManagement/Databases/MigrationHostedService.cs#L48), [CargoOrderingService](../CargoOrderingService/src/CargoOrderingService/Databases/MigrationHostedService.cs#L48)): якщо у збірці є EF Core міграції — `MigrateAsync()`, якщо немає — `EnsureCreatedAsync()`.
2. **Окремий локальний SQL Server (наприклад, контейнер `Sql-Server-2022` на порту 1433/1434) не використовується**, коли сервіси запущено через AppHost: змінні середовища від Aspire перекривають appsettings. Якщо раніше бази створювались у такому сервері через `EnsureCreated`, а потім код намагався застосувати міграції — виникала помилка `There is already an object named '...' in the database`. **Не змішуйте `EnsureCreated` і `Migrate` на одній базі.**

| Хочу… | Як це зробити в AppHost |
| :--- | :--- |
| Зберігати дані між запусками | `builder.AddSqlServer("sqlserver").WithDataVolume()` — дані живуть у Docker-томі |
| Не перезапускати контейнер щоразу (швидший старт) | `.WithLifetime(ContainerLifetime.Persistent)` — контейнер переживає зупинку AppHost |
| Почати «з чистого аркуша» з томом | Видалити том: `docker volume ls` → `docker volume rm <ім'я>` |

> ⚠️ Якщо ви ввімкнете `WithDataVolume()`, пароль `sa` має лишатися стабільним між запусками — Aspire зберігає його в user secrets AppHost. Якщо секрети видалити, SQL Server у старому томі не прийме новий пароль.

### 9.3. Стани ресурсу в дашборді
`Waiting` (чекає на `WaitFor`) → `Starting` → `Running` + Health `Healthy` / `Unhealthy` → `Finished` / `Exited` (процес завершився) / `FailedToStart`. Кнопки ▶/■ у колонці *Actions* дозволяють зупиняти та запускати окремі ресурси — це наш головний інструмент для експериментів зі збоями.

---

<a id="section-10"></a>
## 10. Покроковий практичний чек-лист запуску та діагностики для інженера

### Крок 1. Перевірка вимог оточення
Перед запуском переконайтеся, що на робочій станції запущено Docker-демон та встановлено .NET 10 SDK:
```powershell
docker info
dotnet --version
```
*(У виводі `docker info` повинен відображатися активний статус контейнерного рушія, `dotnet --version` — 10.x).*

### Крок 2. Запуск всієї системи однією командою
У терміналі (з кореня репозиторію) виконайте команду запуску оркестратора:
```powershell
dotnet run --project "02-Microservices-Craftsman\Aspire\CargoDelivery.AppHost\CargoDelivery.AppHost.csproj"
```
Або у Visual Studio / Rider: `CargoDelivery.AppHost` → *Set as Startup Project* → `F5`.

### Крок 3. Що спостерігати в інтерфейсі .NET Aspire Dashboard
1. Після запуску автоматично відкриється вкладка браузера з адресою `https://localhost:17048` (з токеном входу в URL; цей самий URL друкується в консолі як `Login to the dashboard at ...`).
2. Перейдіть у розділ **Resources**:
   * Перевірте, що всі **9 ресурсів** мають стан `Running`, а проєкти — Health `Healthy`.
   * Зверніть увагу на колонку **Source**: для сервісів там вказано шлях до `.csproj`, для контейнерів — назва Docker-образу.
   * Зверніть увагу на колонку **URLs**: кожен мікросервіс має клікабельне посилання, натискання на яке відкриває Swagger (`/swagger`) або цільовий API.
   * Клікніть по рядку `httpaggregator`: у деталях видно *References*, *Health checks* та всі *Environment variables* із §8.
3. Перейдіть до розділу **Console Logs**:
   * Оберіть ресурс `httpaggregator`.
   * Переконайтеся, що під час старту немає жодного повідомлення `SocketException` чи `Connection Refused`.
   * Оберіть `driversmanagement` і знайдіть рядки `Applying migrations for DriversManagementContext` → `Migrations complete` та `Bus started: rabbitmq://localhost:…/`.
4. Виконайте запити з [CargoDelivery.HttpAggregator.http](../CargoDelivery.HttpAggregator/CargoDelivery.HttpAggregator.http) і перейдіть у **Traces** — ви побачите дерево викликів HTTP → gRPC → RabbitMQ → SQL.

---

<a id="section-11"></a>
## 11. Контрольні запитання та практичні завдання

### 🎯 Контрольні запитання:
1. Чому `.WaitFor(projectResource)` без `.WithHttpHealthCheck(...)` гарантує лише запуск процесу, але не готовність сервісу? Що може піти не так у цей проміжок?
2. Поясніть різницю між іменем ресурсу (`driversmanagement-db`), фізичною назвою БД (`dev_driversmanagement`) та `connectionName` (`DriversManagement`). Яка з них потрапляє в змінну середовища?
3. Чому граф залежностей AppHost повинен бути ациклічним? Що станеться, якщо DriversManagement і DriversBlogManagement будуть чекати один на одного?
4. Чому при зупиненому через дашборд сервісі клієнт бачить таймаут, а не миттєве `Connection refused`?
5. Які переваги і ризики дає `WithDataVolume()` для локальної розробки?

### 🛠️ Практичні завдання:
1. Тимчасово приберіть `.WithHttpHealthCheck("/health")` у `driversmanagement` ([AppHost.cs#L101](../Aspire/CargoDelivery.AppHost/AppHost.cs#L101)) і порівняйте, у який момент стартує `httpaggregator` (колонка *Start time*).
2. Додайте `.WithDataVolume()` до `sqlserver`, створіть водія, перезапустіть AppHost і переконайтеся, що дані збереглися.
3. Відкрийте Management UI RabbitMQ і знайдіть прив'язки (Bindings) обмінника `driver-created`.
