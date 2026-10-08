# 🧭 00. Карта та матриця міжсервісних комунікацій (Quick Reference)

> **Модуль:** `02-Microservices-Craftsman / 02-02-Communication-Documentation`  
> **Призначення:** Єдина експрес-карта міжсервісних взаємодій у системі вантажних перевезень. Містить наочні топологічні діаграми та інтерактивні таблиці з прямими клікабельними переходами до вихідного коду кожного клієнта, сервера, продюсера та консьюмера.

---

## 🗺️ 1. Загальна архітектурна топологія системи

У нашій системі реалізовано три взаємодоповнюючі види міжсервісної комунікації:
1. **HTTP Client (Gateway Aggregator / BFF):** паралельна композиція даних для клієнтського дашборду.
2. **gRPC (HTTP/2 Binary RPC):** синхронний високошвидкісний point-to-point обмін між двома сервісами.
3. **Message Broker (MassTransit + RabbitMQ):** асинхронна шина доменних подій для реплікації даних.

```mermaid
flowchart TD
    Client["📱 Frontend / Клієнт"] -->|"GET /api/v1/driver-dashboard/{id}"| DM

    subgraph CoreServices ["⚡ Мікросервіси системи (.NET 10)"]
        DM["🚚 DriversManagement<br/>(BFF Aggregator & Core Domain)"]
        DB["📝 DriversBlogManagement<br/>(Social Domain & gRPC Server)"]
        CO["📦 CargoOrderingService<br/>(Logistics Domain)"]
    end

    subgraph RabbitMQ ["🐰 RabbitMQ Broker (AMQP 0-9-1)"]
        Ex_Created[["Exchange: driver-created (Fanout)"]]
        Ex_Updated[["Exchange: driver-updated (Fanout)"]]
        Ex_Order[["Exchange: order-created (Fanout)"]]
    end

    %% 1. HTTP Aggregator (BFF)
    DM -.->|"1. HTTP GET /api/v1/deliveries (BFF)"| CO
    DM -.->|"1. HTTP GET /api/v1/postaboutdrivers (BFF)"| DB

    %% 2. gRPC RPC
    DM ==>|"2. gRPC GetDriverWithPosts (HTTP/2 Protobuf)"| DB

    %% 3. RabbitMQ EDA
    DM -->|"3. Publish IDriverCreated"| Ex_Created
    DM -->|"3. Publish IDriverUpdated"| Ex_Updated
    CO -->|"3. Publish IOrderCreated"| Ex_Order

    Ex_Created -->|"Consume"| DB
    Ex_Created -->|"Consume"| CO
    Ex_Updated -->|"Consume"| DB
    Ex_Updated -->|"Consume"| CO
```

---

## 🌐 2. Частина 1: HTTP Client Комунікація (Gateway Aggregator / BFF)

### 2.1. Концепція та потік виконання (Fan-Out / Fan-In)

Коли клієнт запитує єдиний зведений екран кабінету водія, контролер `DriverDashboardController` запускає паралельний збір інформації через `Task.WhenAll`:
1. Зчитує локальний агрегат водія з `DriversManagement DB` (EF Core).
2. Викликає `CargoOrderingService` через `CargoOrderingApiClient` для отримання активних рейсів.
3. Викликає `DriversBlogManagement` через `DriversBlogApiClient` для отримання останніх публікацій.
4. Об'єднує результат у єдиний `AggregatedDriverDashboardDto`.

```mermaid
sequenceDiagram
    autonumber
    actor User as 👤 Користувач / UI
    participant Ctrl as 🎮 DriverDashboardController
    participant Hndl as ⚙️ GetDriverDashboard.Handler
    participant LocalDB as 🗄️ Drivers DB (Local)
    participant CO_API as 📦 CargoOrderingService (HTTP)
    participant Blog_API as 📝 DriversBlogManagement (HTTP)

    User->>Ctrl: GET /api/v1/driver-dashboard/{id}
    Ctrl->>Hndl: Send(GetDriverDashboard.Query)
    
    par Паралельний Fan-Out (Task.WhenAll)
        Hndl->>LocalDB: GetById(id)
        LocalDB-->>Hndl: Driver Aggregate Entity
    and
        Hndl->>CO_API: GET /api/v1/deliveries
        CO_API-->>Hndl: 200 OK (List of Deliveries)
    and
        Hndl->>Blog_API: GET /api/v1/postaboutdrivers
        alt Блог доступний
            Blog_API-->>Hndl: 200 OK (List of Posts)
        else Сервіс офлайн / таймаут
            Hndl->>Hndl: Fallback: порожній список []
        end
    end

    Note over Hndl: Fan-In: Злиття даних у AggregatedDriverDashboardDto
    Hndl-->>Ctrl: 200 OK зведений DTO
    Ctrl-->>User: 200 OK JSON відповідь
```

### 2.2. Навігаційна таблиця HTTP-клієнтів та ендпоінтів

| Роль у виклику | Проєкт | Компонент / Клас | Метод / Маршрут | Опис та призначення |
| :--- | :--- | :--- | :--- | :--- |
| **Точка входу BFF** | `DriversManagement` | [DriverDashboardController](../DriversManagement/src/DriversManagement/Controllers/v1/DriverDashboardController.cs) | `GET /api/v1/driver-dashboard/{id}` | Зовнішній REST-ендпоінт агрегації для мобільного/веб клієнта |
| **Оркестратор BFF** | `DriversManagement` | [GetDriverDashboard.cs](../DriversManagement/src/DriversManagement/Domain/Drivers/Features/GetDriverDashboard.cs) | `GetDriverDashboard.Handler` | CQRS-обробник, який виконує паралельний `Task.WhenAll` та злиття даних |
| **HTTP-клієнт замовлень** | `DriversManagement` | [CargoOrderingApiClient.cs](../DriversManagement/src/DriversManagement/Domain/Drivers/Services/Http/CargoOrderingApiClient.cs)<br/>([ICargoOrderingApiClient](../DriversManagement/src/DriversManagement/Domain/Drivers/Services/Http/ICargoOrderingApiClient.cs)) | `GetActiveDeliveriesByDriverAsync()` | Типізований клієнт з `AddStandardResilienceHandler()` та Aspire Service Discovery |
| **HTTP-клієнт блогу** | `DriversManagement` | [DriversBlogApiClient.cs](../DriversManagement/src/DriversManagement/Domain/Drivers/Services/Http/DriversBlogApiClient.cs)<br/>([IDriversBlogApiClient](../DriversManagement/src/DriversManagement/Domain/Drivers/Services/Http/IDriversBlogApiClient.cs)) | `GetRecentPostsByDriverAsync()` | Типізований клієнт з обробкою таймаутів та Graceful Fallback |
| **Зведена модель** | `DriversManagement` | [AggregatedDriverDashboardDto.cs](../DriversManagement/src/DriversManagement/Domain/Drivers/Dtos/AggregatedDriverDashboardDto.cs) | `AggregatedDriverDashboardDto` | Композитна DTO-модель (Driver + Truck + Deliveries + Posts) |
| **DI Конфігурація** | `DriversManagement` | [WebAppServiceConfiguration.cs](../DriversManagement/src/DriversManagement/Extensions/Services/WebAppServiceConfiguration.cs) | `AddHttpClient(...)` | Реєстрація клієнтів з базовими адресами `http://cargoorderingservice` та `http://driversblogmanagement` |
| **Цільовий сервіс 1** | `CargoOrderingService` | [DeliveriesController.cs](../CargoOrderingService/src/CargoOrderingService/Controllers/v1/DeliveriesController.cs) | `GET /api/v1/deliveries` | Надає список рейсів і доставок із пагінацією |
| **Цільовий сервіс 2** | `DriversBlogManagement` | [PostAboutDriversController.cs](../DriversBlogManagement/src/DriversBlogManagement/Controllers/v1/PostAboutDriversController.cs) | `GET /api/v1/postaboutdrivers` | Надає список дописів водіїв із пагінацією |

> 📖 **Детальний навчальний гайд:** [02_Patterns_Synchronous_HttpClient_Aggregator_BFF.md](02_Patterns_Synchronous_HttpClient_Aggregator_BFF.md)

---

## ⚡ 3. Частина 2: gRPC Комунікація (Синхронний бінарний RPC по HTTP/2)

### 3.1. Архітектура з'єднання та Graceful Degradation

gRPC забезпечує ультранизьку затримку (Low Latency) та компактний бінарний формат передачі даних:
- **Клієнт:** `DriversManagement` через згенерований `DriverRpc.DriverRpcClient`.
- **Сервер:** `DriversBlogManagement` через клас `DriverRpcService`.
- **Захист від каскадного збою:** Якщо gRPC-сервер тимчасово недоступний, клієнт логує `RpcException` та повертає водія з порожнім списком публікацій (`Fallback`), не ламаючи весь запит користувача!

```mermaid
sequenceDiagram
    autonumber
    participant ClientApp as 🚚 DriversManagement (Client)
    participant Handler as ⚙️ GetDriver.Handler
    participant gRPC_Client as 🔌 DriverRpc.DriverRpcClient
    participant gRPC_Server as 📝 DriverRpcService (Server)
    participant BlogDB as 🗄️ DriversBlog DB

    ClientApp->>Handler: Send(GetDriver.Query(id))
    Handler->>gRPC_Client: GetDriverWithPostsAsync(request)
    
    rect rgb(20, 35, 60)
        note over gRPC_Client,gRPC_Server: HTTP/2 Binary Frames (Protocol Buffers)
        gRPC_Client->>gRPC_Server: POST /DriverRpc/GetDriverWithPosts
        gRPC_Server->>BlogDB: Читання постів водія
        BlogDB-->>gRPC_Server: Список PostAboutDrivers
        gRPC_Server-->>gRPC_Client: DriverWithPostsResponse (Binary)
    end

    alt Успішний виклик
        gRPC_Client-->>Handler: DriverWithPostsResponse.Posts
    else Помилка сервера (RpcException / Unavailable)
        gRPC_Client--xHandler: RpcException
        Handler->>Handler: Fallback: повернути водія з posts = []
    end

    Handler-->>ClientApp: FullDataAboutDriverAndPostsDto
```

### 3.2. Навігаційна таблиця компонентів gRPC

| Сторона взаємодії | Проєкт | Компонент / Клас | Файл / Контракт | Призначення |
| :--- | :--- | :--- | :--- | :--- |
| **Контракт Protobuf** | Спільний | `Domain/Drivers/Protos/Driver.proto` | [Driver.proto](../DriversBlogManagement/src/DriversBlogManagement/Domain/Drivers/Protos/Driver.proto) | Бінарний IDL-контракт: `rpc GetDriverWithPosts`, повідомлення `DriverWithPostsRequest` та `DriverWithPostsResponse` |
| **Генерація коду** | `DriversBlogManagement` | [DriversBlogManagement.csproj](../DriversBlogManagement/src/DriversBlogManagement/DriversBlogManagement.csproj) | `<Protobuf GrpcServices="Both" />` | Компілятор Protobuf генерує клієнтські та серверні класи C# |
| **gRPC Клієнт (Виклик)** | `DriversManagement` | [GetDriver.cs](../DriversManagement/src/DriversManagement/Domain/Drivers/Features/GetDriver.cs) | `GetDriver.Handler` | Викликає `_driverRpcClient.GetDriverWithPostsAsync` та інкапсулює Graceful Degradation `try/catch (RpcException)` |
| **Реєстрація клієнта** | `DriversManagement` | [WebAppServiceConfiguration.cs](../DriversManagement/src/DriversManagement/Extensions/Services/WebAppServiceConfiguration.cs) | `AddGrpcClient<DriverRpcClient>` | Читає адресу з Aspire Service Discovery (`services:driversblogmanagement:https:0`) або Fallback |
| **gRPC Сервер (Реалізація)** | `DriversBlogManagement` | [DriverRpcService.cs](../DriversBlogManagement/src/DriversBlogManagement/Domain/Drivers/Services/DriverRpcService.cs) | `DriverRpcService : DriverRpcBase` | Серверна реалізація, що робить запит до локальної БД `DriversBlog` та мапить результати в Protobuf |
| **Реєстрація сервера** | `DriversBlogManagement` | [Program.cs](../DriversBlogManagement/src/DriversBlogManagement/Program.cs) | `app.MapGrpcService<DriverRpcService>()` | Підключення gRPC-конвеєра в ASP.NET Core Kestrel |

> 📖 **Детальний навчальний гайд:** [03_Patterns_Synchronous_gRPC_Communication.md](03_Patterns_Synchronous_gRPC_Communication.md)

---

## 🐰 4. Частина 3: Message Broker (MassTransit + RabbitMQ Pub/Sub)

### 4.1. Топологія брокера: Обмінники (Fanout) та Черги (Quorum)

В асинхронній моделі сервіс-видавець не знає, хто підписаний на його події:
- Продюсер публікує подію в **Обмінник (Exchange)** типу `Fanout`.
- RabbitMQ миттєво копіює повідомлення в усі зв'язані **Черги (Queues)**.
- Сервіси-підписники вичитують повідомлення автономно та ідемпотентно.

```mermaid
flowchart LR
    subgraph Publishers ["🚀 Продюсери подій"]
        DM_Add["AddDriver.cs<br/>(DriversManagement)"]
        DM_Upd["UpdateDriver.cs<br/>(DriversManagement)"]
        CO_Ord["AddOrder.cs<br/>(CargoOrderingService)"]
    end

    subgraph Exchanges ["🐰 RabbitMQ Fanout Exchanges"]
        Ex1[["driver-created"]]
        Ex2[["driver-updated"]]
        Ex3[["order-created"]]
    end

    subgraph Queues ["📥 Quorum Queues"]
        Q_Blog[("sync-updated-driver<br/>(Блог водіїв)")]
        Q_Ord[("ordering-sync-driver<br/>(Замовлення)")]
    end

    subgraph Consumers ["📝 Споживачі подій"]
        Cons_Blog["SyncDriver.cs<br/>(DriversBlogManagement)"]
        Cons_Ord["SyncDriverInOrdering.cs<br/>(CargoOrderingService)"]
    end

    DM_Add -->|Publish IDriverCreated| Ex1
    DM_Upd -->|Publish IDriverUpdated| Ex2
    CO_Ord -->|Publish IOrderCreated| Ex3

    Ex1 -->|Binding| Q_Blog
    Ex1 -->|Binding| Q_Ord
    Ex2 -->|Binding| Q_Blog
    Ex2 -->|Binding| Q_Ord

    Q_Blog --> Cons_Blog
    Q_Ord --> Cons_Ord
```

### 4.2. Навігаційна таблиця продюсерів, черг та споживачів

| Подія / Інтерфейс | Продюсер (Хто публікує) | Обмінник (Exchange) | Черга (Queue) | Споживач (Хто вичитує) | Що відбувається в БД |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **`IDriverCreated`**<br/>([DriverCreated.cs](../SharedKernel/DriverCreated.cs)) | `DriversManagement`<br/>[AddDriver.cs](../DriversManagement/src/DriversManagement/Domain/Drivers/Features/AddDriver.cs)<br/>*Реєстрація:* [DriverCreatedEndpointRegistration.cs](../DriversManagement/src/DriversManagement/Extensions/Services/ProducerRegistrations/DriverCreatedEndpointRegistration.cs) | `driver-created`<br/>*(Type: Fanout)* | `sync-updated-driver` | `DriversBlogManagement`<br/>[SyncDriver.cs](../DriversBlogManagement/src/DriversBlogManagement/Domain/SyncDriver.cs)<br/>*Реєстрація:* [SyncPatientEndpointRegistration.cs](../DriversBlogManagement/src/DriversBlogManagement/Extensions/Services/ConsumerRegistrations/SyncPatientEndpointRegistration.cs) | Створення тіньового запису водія в базі блогу (`DriversBlog.Drivers`) |
| **`IDriverCreated`**<br/>([DriverCreated.cs](../SharedKernel/DriverCreated.cs)) | `DriversManagement`<br/>[AddDriver.cs](../DriversManagement/src/DriversManagement/Domain/Drivers/Features/AddDriver.cs) | `driver-created`<br/>*(Type: Fanout)* | `ordering-sync-driver` | `CargoOrderingService`<br/>[SyncDriverInOrdering.cs](../CargoOrderingService/src/CargoOrderingService/Domain/Drivers/Consumers/SyncDriverInOrdering.cs)<br/>*Реєстрація:* [SyncDriverInOrderingEndpointRegistration.cs](../CargoOrderingService/src/CargoOrderingService/Extensions/Services/ConsumerRegistrations/SyncDriverInOrderingEndpointRegistration.cs) | Створення тіньового запису водія в базі замовлень (`Ordering.Drivers`) |
| **`IDriverUpdated`**<br/>([DriverUpdated.cs](../SharedKernel/DriverUpdated.cs)) | `DriversManagement`<br/>[UpdateDriver.cs](../DriversManagement/src/DriversManagement/Domain/Drivers/Features/UpdateDriver.cs)<br/>*Реєстрація:* [DriverUpdatedEndpointRegistration.cs](../DriversManagement/src/DriversManagement/Extensions/Services/ProducerRegistrations/DriverUpdatedEndpointRegistration.cs) | `driver-updated`<br/>*(Type: Fanout)* | `sync-updated-driver` | `DriversBlogManagement`<br/>[SyncDriver.cs](../DriversBlogManagement/src/DriversBlogManagement/Domain/SyncDriver.cs) | Оновлення ПІБ водія в базі блогу для відображення під постами |
| **`IDriverUpdated`**<br/>([DriverUpdated.cs](../SharedKernel/DriverUpdated.cs)) | `DriversManagement`<br/>[UpdateDriver.cs](../DriversManagement/src/DriversManagement/Domain/Drivers/Features/UpdateDriver.cs) | `driver-updated`<br/>*(Type: Fanout)* | `ordering-sync-driver` | `CargoOrderingService`<br/>[SyncDriverInOrdering.cs](../CargoOrderingService/src/CargoOrderingService/Domain/Drivers/Consumers/SyncDriverInOrdering.cs) | Оновлення контактних даних водія в базі замовлень |
| **`IOrderCreated`**<br/>([OrderMessages.cs](../SharedKernel/OrderMessages.cs)) | `CargoOrderingService`<br/>[AddOrder.cs](../CargoOrderingService/src/CargoOrderingService/Domain/Orders/Features/AddOrder.cs) | `order-created`<br/>*(Type: Fanout)* | Підготовлена для нових сервісів | Розширюваний контракт для майбутніх модулів (Billing / Notifications) | Фіксація факту оформлення нового рейсу |

#### Конфігурація підключення шини у кожному мікросервісі:
* 🚚 **DriversManagement:** [MassTransitServiceExtension.cs](../DriversManagement/src/DriversManagement/Extensions/Services/MassTransitServiceExtension.cs)
* 📝 **DriversBlogManagement:** [MassTransitServiceExtension.cs](../DriversBlogManagement/src/DriversBlogManagement/Extensions/Services/MassTransitServiceExtension.cs)
* 📦 **CargoOrderingService:** [MassTransitServiceExtension.cs](../CargoOrderingService/src/CargoOrderingService/Extensions/Services/MassTransitServiceExtension.cs)

> 📖 **Детальний навчальний гайд:** [04_Patterns_Asynchronous_Messaging_MassTransit_RabbitMQ.md](04_Patterns_Asynchronous_Messaging_MassTransit_RabbitMQ.md)

---

## ⚖️ 5. Зведена матриця вибору патерну комунікації

| Критерій | 1. HTTP Aggregator (BFF) | 2. gRPC (HTTP/2 Binary RPC) | 3. MassTransit + RabbitMQ (Pub/Sub) |
| :--- | :--- | :--- | :--- |
| **Тип взаємодії** | Синхронний запит / відповідь | Синхронний point-to-point RPC | Асинхронний подійний Pub/Sub |
| **Протокол передачі** | HTTP/1.1 або HTTP/2 + JSON | HTTP/2 + Google Protocol Buffers | AMQP 0-9-1 поверх TCP |
| **Формат даних** | Текстовий JSON | Бінарні спресовані фрейми | JSON / Protobuf у тілі повідомлення |
| **Швидкість (Latency)** | Середня (парсинг тексту JSON) | **Надвисока** (бінарна серіалізація) | Висока (зберігання брокером на диск) |
| **Зв'язність за часом** | Висока (потрібні відповіді API) | Висока (сервер повинен бути онлайн) | **Нульова** (Temporal Decoupling) |
| **Вплив збою адресата** | Частковий Fallback для блоків UI | Помилка збою / Graceful Degradation | **Повідомлення чекає в черзі брокера** |
| **Головний кейс у системі** | Зведення даних для клієнта (BFF Dashboard) | Швидке зчитування постів водія | Реплікація водіїв між базами даних |
| **Повний гайд** | [Гайд 02](02_Patterns_Synchronous_HttpClient_Aggregator_BFF.md) | [Гайд 03](03_Patterns_Synchronous_gRPC_Communication.md) | [Гайд 04](04_Patterns_Asynchronous_Messaging_MassTransit_RabbitMQ.md) |
