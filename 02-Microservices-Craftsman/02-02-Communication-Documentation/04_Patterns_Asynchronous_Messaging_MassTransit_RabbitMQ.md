# 📬 04. Патерни асинхронного обміну подіями: MassTransit, RabbitMQ та Eventual Consistency

> **Блок 2:** `02-Microservices-Craftsman`  
> **Технологічний стек:** .NET 10, MassTransit 8.1.3, RabbitMQ (AMQP 0-9-1, Fanout Exchanges, Quorum Queues, Lazy Queues), SharedKernel, InMemory Fallback Bus.

---

## 1. Теоретична концепція: Подієво-орієнтована архітектура (Event-Driven Architecture)

У синхронних моделях (розглянутих у [Гайді 02: HTTP Gateway Aggregator](02_Patterns_Synchronous_HttpClient_Aggregator_BFF.md) та [Гайді 03: gRPC](03_Patterns_Synchronous_gRPC_Communication.md)) клієнт або зухвалий сервіс **блокується й чекає**, поки віддалений сервіс завершить обчислення та надішле відповідь.

Якщо цільовий сервіс недоступний, перезавантажується, виконує міграцію бази даних або тимчасово перевантажений:
* Синхронний запит зазнає збою (Exception / HTTP 500 / 504 Gateway Timeout).
* Починається каскадний колапс суміжних підсистем (**Cascading Failure**).
* Сервіси стають жорстко прив'язаними один до одного за часом виконання (**Temporal Coupling**).

**Подієво-орієнтована архітектура (EDA)** повністю вирішує цю проблему за допомогою **часового та просторового розв'язання (Temporal & Spatial Decoupling)**:
1. **Факт у минулому часі:** Сервіс-джерело публікує факт події, що вже сталася в його бізнес-домені (`DriverCreated`, `DriverUpdated`, `OrderCreated`).
2. **Гарантія брокера:** Брокер повідомлень (**RabbitMQ**) негайно зберігає повідомлення на диск у захищену чергу та повертає підтвердження публікації.
3. **Автономність споживачів:** Сервіси-підписники вичитують події у власному темпі, коли вони готові, без необхідності тримати пряме з'єднання з видавцем.

```mermaid
sequenceDiagram
    autonumber
    
    rect rgb(38, 25, 25)
        note over Client,SyncServer: ❌ 1. Синхронна взаємодія (REST / gRPC) — Часове блокування (Temporal Coupling)
        Client->>SyncServer: Виклик: POST /api/v1/drivers (або gRPC)
        activate Client
        activate SyncServer
        note over SyncServer: Сервер виконує бізнес-логіку,<br/>запис у базу даних тощо...
        note over Client: ⏳ Клієнт заблокований і чекає!<br/>(Якщо сервер офлайн або впав — клієнт ламається)
        SyncServer-->>Client: Відповідь: 200 OK / Response DTO
        deactivate SyncServer
        deactivate Client
    end

    rect rgb(20, 36, 48)
        note over Producer,Consumer: ✅ 2. Асинхронний EDA (MassTransit + RabbitMQ) — Часове розв'язання (Temporal Decoupling)
        Producer->>Broker: Публікація: Publish<IDriverCreated>(event)
        activate Producer
        activate Broker
        Broker-->>Producer: Basic.Ack (Подія збережена в Quorum Queue)
        deactivate Broker
        deactivate Producer
        note over Producer: 🚀 Продюсер миттєво вільний!<br/>Диспетчер отримує 201 Created за 10мс.

        note over Broker,Consumer: Асинхронна доставка (у власному темпі підписника)
        Broker->)Consumer: Push повідомлення з черги: ConsumeContext<IDriverCreated>
        activate Consumer
        note over Consumer: Споживач записує дані у свою БД,<br/>навіть якщо продюсер уже вимкнено
        Consumer-->>Broker: Basic.Ack (Повідомлення видалено з черги)
        deactivate Consumer
    end
```

---

## 2. Аналогія з реального життя: Поштовий термінал логістичної компанії

* **Синхронний телефонний дзвінок (REST / gRPC):** Диспетчер телефонує водієві: *"Ти прийняв вантаж? Скажи мені прямо зараз!"*. Якщо у водія розрядився телефон, він у тунелі або зайнятий керуванням — дзвінок зривається, а робота диспетчера блокується.
* **Асинхронний поштовий термінал (MassTransit + RabbitMQ):** Диспетчер друкує наказ на завантаження, кладе його у центральний сортувальний термінал (**Exchange**). Сортувальний автомат миттєво робить копії наказу для бухгалтерії, складу та гаража і розкладає їх по персональних закритих скриньках (**Queues**). Диспетчер миттєво повертається до інших справ. Коли комірник виходить на зміну, він відкриває свою скриньку, забирає наказ і спокійно комплектує вантаж.

---

## 3. Модель протоколу AMQP 0-9-1 та топологія RabbitMQ

RabbitMQ працює за стандартом **AMQP 0-9-1** (Advanced Message Queuing Protocol). У цьому протоколі повідомлення **ніколи не надсилаються безпосередньо в чергу** — вони завжди надсилаються в **Обмінник (Exchange)**:

```mermaid
flowchart LR
    subgraph Publisher["🚚 DriversManagement"]
        P["IPublishEndpoint"]
    end

    subgraph RabbitMQ["🐰 RabbitMQ Broker (AMQP 0-9-1)"]
        direction TB
        Ex1[["Exchange: driver-created<br/>(Type: Fanout)"]]
        Ex2[["Exchange: driver-updated<br/>(Type: Fanout)"]]

        Q1[("Queue: sync-updated-driver<br/>(Quorum Queue / Lazy)")]
        Q2[("Queue: ordering-sync-driver<br/>(Quorum Queue / Lazy)")]

        Ex1 -->|Binding| Q1
        Ex2 -->|Binding| Q1

        Ex1 -->|Binding| Q2
        Ex2 -->|Binding| Q2
    end

    subgraph BlogConsumer["📝 DriversBlogManagement"]
        Q1 --> C1["SyncDriver.cs<br/>(IConsumer)"]
    end

    subgraph OrderingConsumer["📦 CargoOrderingService"]
        Q2 --> C2["SyncDriverInOrdering.cs<br/>(IConsumer)"]
    end

    P -->|IDriverCreated| Ex1
    P -->|IDriverUpdated| Ex2
```

### Чому обрано саме тип обмінника Fanout?
| Тип Exchange | Механізм маршрутизації | Коли застосовується | Чому Fanout у нашому проєкті? |
| :--- | :--- | :--- | :--- |
| **Fanout** | **Широкомовний (Broadcasting).** Ігнорує Routing Key, дублює повідомлення в **усі** прив'язані черги. | Pub-Sub доменних подій усім підписникам. | **Ідеально для реплікації водіїв.** Коли з'являється або оновлюється водій, усі зацікавлені мікросервіси повинні гарантовано отримати копію події. |
| **Direct** | Точний збіг Routing Key (`RoutingKey == BindingKey`). | Таргетована відправка команд конкретному воркеру. | Не підходить для широкомовного сповіщення кількох сервісів. |
| **Topic** | Збіг за маскою із зірочками (`cargo.eu.*`, `orders.#`). | Складні ієрархічні фільтри маршрутизації. | Надлишковий для загальних доменних подій створення/оновлення. |
| **Headers** | Маршрутизація за заголовками AMQP, а не за Routing Key. | Специфічні метадані (формати, мови). | Рідко використовується на практиці через накладні витрати. |

---

## 4. Кворум-черги (Quorum Queues) та оптимізація Lazy Queues

У конфігурації обох споживачів (`ordering-sync-driver` та `sync-updated-driver`) застосовано налаштування корпоративного рівня:

```csharp
re.SetQuorumQueue();
re.SetQueueArgument("declare", "lazy");
```

### 1. Що дає `SetQuorumQueue()`?
* **Raft Consensus:** Quorum Queues базуються на консенсусному алгоритмі **Raft** (на відміну від застарілих Mirrored Queues).
* **Стійкість до втрати даних:** Повідомлення реплікується на кворум вузлів кластера RabbitMQ. Навіть при аварійній зупинці або перезавантаженні брокера жодна подія про створення чи зміну водія не загубиться.
* **FIFO порядок:** Гарантується суворий порядок збереження черги.

### 2. Що дає `lazy` аргумент (`SetQueueArgument("declare", "lazy")`)?
* **Зниження навантаження на RAM:** Повідомлення одразу записуються на диск та вивантажуються з оперативної пам'яті брокера, якщо черга не вичитується миттєво.
* **Передбачуваність під піковими навантаженнями:** Брокер не відчуває раптового вичерпання RAM (OOM Killer) при масовому надходженні тисяч замовлень чи водіїв.

---

## 5. Діаграми процесів взаємодії

### 5.1. Повна діаграма послідовності виклику (Sequence Diagram: Publish -> Broker -> Consumers)

```mermaid
sequenceDiagram
    autonumber
    actor Dispatcher as 👤 Диспетчер / Користувач
    participant DM as 🚚 DriversManagement (API)
    participant DM_DB as 🗄️ DriversManagement DB
    participant Bus as 🚌 MassTransit (IPublishEndpoint)
    participant Broker as 🐰 RabbitMQ (Fanout Exchange)
    participant Q_Blog as 📥 Queue: sync-updated-driver
    participant Q_Ord as 📥 Queue: ordering-sync-driver
    participant Blog as 📝 DriversBlogManagement (Consumer)
    participant Ord as 📦 CargoOrderingService (Consumer)

    Dispatcher->>DM: POST /api/v1/drivers (Створення водія)
    DM->>DM_DB: INSERT INTO Drivers; CommitChanges();
    DM_DB-->>DM: 200 OK (Водія збережено локально)

    %% Асинхронна публікація події
    rect rgb(20, 35, 60)
        note over DM,Broker: Асинхронна публікація доменної події
        DM->>Bus: Publish<IDriverCreated>(new DriverCreated { ... })
        Bus->>Broker: Basic.Publish -> exchange "driver-created"
        Broker-->>Bus: Basic.Ack (Повідомлення прийнято брокером)
        DM-->>Dispatcher: 201 Created (Миттєва відповідь диспетчеру!)
    end

    %% Брокер розмножує повідомлення
    Broker->>Q_Blog: Дублювання у чергу блогу
    Broker->>Q_Ord: Дублювання у чергу замовлень

    %% Асинхронне вичитування підписниками
    par Споживач Блогу
        Q_Blog->>Blog: ConsumeContext<IDriverCreated>
        Blog->>Blog: Перевірка ідемпотентності (if not exists)
        Blog->>Blog: INSERT INTO Shadow Drivers (EF Core)
        Blog-->>Q_Blog: Basic.Ack (Повідомлення видалено з черги)
    and Споживач Замовлень
        Q_Ord->>Ord: ConsumeContext<IDriverCreated>
        Ord->>Ord: Перевірка ідемпотентності (if not exists)
        Ord->>Ord: INSERT INTO Shadow Drivers (EF Core)
        Ord-->>Q_Ord: Basic.Ack (Повідомлення видалено з черги)
    end
```

---

### 5.2. Діаграма життєвого циклу повідомлення у MassTransit (Message Lifecycle & Dead Letter)

Що відбувається, коли споживач падає з помилкою (наприклад, тимчасово недоступна база даних блогу)?

```mermaid
stateDiagram-v2
    [*] --> InQueue: Повідомлення надійшло в чергу (Ready)

    InQueue --> Processing: Споживач отримав (Unacked)
    
    Processing --> Acknowledged: Успішна обробка (Consume completed)
    note right of Acknowledged: Надсилається Basic.Ack.<br/>Повідомлення видаляється з черги.

    Processing --> RetrySchedule: Виняток (DbUpdateException / Timeout)
    note right of RetrySchedule: MassTransit застосовує політики Retry:<br/>Негайний повтор або Exponential Backoff

    RetrySchedule --> Processing: Повторна спроба обробки
    RetrySchedule --> ErrorQueue: Вичерпано ліміт спроб (Poison Message)
    note right of ErrorQueue: Повідомлення переміщується в чергу помилок<br/>"ordering-sync-driver_error"<br/>для подальшого аналізу інженерами.

    Acknowledged --> [*]
    ErrorQueue --> [*]
```

---

## 6. Проблема подвійного запису (Dual-Write Problem) та Eventual Consistency

У розподілених системах без розподілених двофазних блокуючих транзакцій (2PC / WS-AtomicTransaction) існує класична проблема:

```csharp
// 1. Локальна транзакція
await _unitOfWork.CommitChanges(cancellationToken);

// 2. Мережевий запит до брокера
await _publishEndpoint.Publish<IDriverCreated>(message, cancellationToken);
```

### Що може піти не так?
1. База даних зафіксувала водія, але сервер аварійно вимкнувся прямо перед `Publish`. У результаті: водій існує в `DriversManagement`, але жоден інший мікросервіс ніколи не дізнається про це.
2. `Publish` виконано першим, але `CommitChanges` впав через порушення констрейнта. У результаті: підписники отримали подію про водія, якого насправді не існує (**Ghost Entity**).

### Як це вирішується в нашому проєкті?
* **Принцип Eventual Consistency (Кінцева узгодженість):** База даних є первинним джерелом правди. Повідомлення публікується **після** успішного фіксування транзакції `_unitOfWork.CommitChanges`.
* **Ідемпотентність споживачів:** Усі споживачі перевіряють наявність сутності перед вставкою (`if (existingDriver == null)`), що запобігає збоям при випадкових повторах повідомлень.
* **Перспектива розвитку — Transactional Outbox Pattern:** У наступних ітераціях можна увімкнути нативний `AddEntityFrameworkOutbox<TContext>()` від MassTransit, який зберігає повідомлення у спеціальну таблицю `OutboxMessages` в тій самій транзакції EF Core, а фоновий воркер MassTransit гарантовано пересилає їх у RabbitMQ.

---

## 7. Детальний розбір реалізації в коді проєкту

### 7.1. Спільні контракти повідомлень (SharedKernel)

Контракти повідомлень містяться у легкому проєкті `SharedKernel` без залежностей від баз даних:

> 📍 **Шлях до файлу:** `SharedKernel/DriverCreated.cs`  
> 📝 *У файлі наведено докладні покрокові коментарі українською мовою.*

[👉 **Перейти до файлу (клікніть тут)**](../SharedKernel/DriverCreated.cs#L26-L52)

```csharp
public interface IDriverCreated
{
    Guid DriverId { get; set; }
    string FirstName { get; set; }
    string LastName { get; set; }
    string DriverLicenseNumber { get; set; }
    string Phone { get; set; }
}
```

> 📍 **Шлях до файлу:** `SharedKernel/DriverUpdated.cs`  
> 📝 *У файлі наведено докладні покрокові коментарі українською мовою.*

[👉 **Перейти до файлу (клікніть тут)**](../SharedKernel/DriverUpdated.cs#L20-L36)

```csharp
public interface IDriverUpdated
{
    Guid DriverId { get; set; }
    string FirstName { get; set; }
    string LastName { get; set; }
}
```

> 📍 **Шлях до файлу:** `SharedKernel/OrderMessages.cs`  
> 📝 *У файлі наведено докладні покрокові коментарі українською мовою.*

[👉 **Перейти до файлу (клікніть тут)**](../SharedKernel/OrderMessages.cs#L20-L40)

---

### 7.2. Продюсер подій: сервіс DriversManagement

Коли створюється водій, після збереження агрегату в базі викликається `IPublishEndpoint.Publish`:

> 📍 **Шлях до файлу:** `DriversManagement/src/DriversManagement/Domain/Drivers/Features/AddDriver.cs`  
> 📝 *У файлі наведено докладні покрокові коментарі українською мовою.*

[👉 **Перейти до файлу (клікніть тут)**](../DriversManagement/src/DriversManagement/Domain/Drivers/Features/AddDriver.cs#L45-L65)

```csharp
// Зберігаємо водія в локальній базі даних
await _driverRepository.Add(driver, cancellationToken);
await _unitOfWork.CommitChanges(cancellationToken);

// Публікуємо доменну подію створення водія в RabbitMQ
await _publishEndpoint.Publish<IDriverCreated>(new DriverCreated
{
    DriverId = driver.Id,
    FirstName = driver.FirstName,
    LastName = driver.LastName,
    DriverLicenseNumber = driver.DriverLicenseNumber,
    Phone = driver.Phone
}, cancellationToken);
```

При оновленні даних водія аналогічно публікується подія `IDriverUpdated`:

> 📍 **Шлях до файлу:** `DriversManagement/src/DriversManagement/Domain/Drivers/Features/UpdateDriver.cs`  
> 📝 *У файлі наведено докладні покрокові коментарі українською мовою.*

[👉 **Перейти до файлу (клікніть тут)**](../DriversManagement/src/DriversManagement/Domain/Drivers/Features/UpdateDriver.cs#L45-L65)

---

### 7.3. Налаштування первинних обмінників (Producer Registrations)

У конфігурації шини ми явно прив'язуємо інтерфейси до імен обмінників типу `Fanout`:

> 📍 **Шлях до файлу:** `DriversManagement/src/DriversManagement/Extensions/Services/ProducerRegistrations/DriverCreatedEndpointRegistration.cs`  
> 📝 *У файлі наведено докладні покрокові коментарі українською мовою.*

[👉 **Перейти до файлу (клікніть тут)**](../DriversManagement/src/DriversManagement/Extensions/Services/ProducerRegistrations/DriverCreatedEndpointRegistration.cs#L8-L16)

```csharp
public static void DriverCreatedEndpoint(this IRabbitMqBusFactoryConfigurator cfg)
{
    cfg.Message<IDriverCreated>(e => e.SetEntityName("driver-created"));
    cfg.Publish<IDriverCreated>(e => e.ExchangeType = ExchangeType.Fanout);
}
```

> 📍 **Шлях до файлу:** `DriversManagement/src/DriversManagement/Extensions/Services/ProducerRegistrations/DriverUpdatedEndpointRegistration.cs`  
> 📝 *У файлі наведено докладні покрокові коментарі українською мовою.*

[👉 **Перейти до файлу (клікніть тут)**](../DriversManagement/src/DriversManagement/Extensions/Services/ProducerRegistrations/DriverUpdatedEndpointRegistration.cs#L8-L16)

---

### 7.4. Споживач у блозі водіїв (DriversBlogManagement)

Клас `SyncDriver` підписаний одночасно на створення та оновлення водіїв:

> 📍 **Шлях до файлу:** `DriversBlogManagement/src/DriversBlogManagement/Domain/SyncDriver.cs`  
> 📝 *У файлі наведено докладні покрокові коментарі українською мовою щодо ідемпотентності та тіньових моделей.*

[👉 **Перейти до файлу (клікніть тут)**](../DriversBlogManagement/src/DriversBlogManagement/Domain/SyncDriver.cs#L50-L80)

```csharp
public sealed class SyncDriver : IConsumer<IDriverCreated>, IConsumer<IDriverUpdated>
{
    public async Task Consume(ConsumeContext<IDriverCreated> context)
    {
        var msg = context.Message;
        var existingDriver = await _db.Drivers.FirstOrDefaultAsync(d => d.Id == msg.DriverId, context.CancellationToken);
        if (existingDriver == null)
        {
            var driver = Driver.CreateWithId(msg.DriverId, msg.FirstName, msg.LastName);
            await _db.Drivers.AddAsync(driver, context.CancellationToken);
            await _db.SaveChangesAsync(context.CancellationToken);
        }
    }
}
```

Конфігурація кінцевої точки та зв'язування з обмінниками:

> 📍 **Шлях до файлу:** `DriversBlogManagement/src/DriversBlogManagement/Extensions/Services/ConsumerRegistrations/SyncPatientEndpointRegistration.cs`  
> 📝 *У файлі наведено докладні покрокові коментарі українською мовою.*

[👉 **Перейти до файлу (клікніть тут)**](../DriversBlogManagement/src/DriversBlogManagement/Extensions/Services/ConsumerRegistrations/SyncPatientEndpointRegistration.cs#L10-L35)

---

### 7.5. Споживач у сервісі замовлень (CargoOrderingService)

Сервіс замовлень також реплікує водіїв для забезпечення повної автономності при призначенні вантажів:

> 📍 **Шлях до файлу:** `CargoOrderingService/src/CargoOrderingService/Domain/Drivers/Consumers/SyncDriverInOrdering.cs`  
> 📝 *У файлі наведено докладні покрокові коментарі українською мовою щодо автономності Bounded Context.*

[👉 **Перейти до файлу (клікніть тут)**](../CargoOrderingService/src/CargoOrderingService/Domain/Drivers/Consumers/SyncDriverInOrdering.cs#L50-L80)

> 📍 **Шлях до файлу:** `CargoOrderingService/src/CargoOrderingService/Extensions/Services/ConsumerRegistrations/SyncDriverInOrderingEndpointRegistration.cs`  
> 📝 *У файлі наведено докладні покрокові коментарі українською мовою.*

[👉 **Перейти до файлу (клікніть тут)**](../CargoOrderingService/src/CargoOrderingService/Extensions/Services/ConsumerRegistrations/SyncDriverInOrderingEndpointRegistration.cs#L10-L30)

---

### 7.6. Відмовостійкість та In-Memory Transport Fallback

У файлі конфігурації шини реалізовано інтелектуальний механізм вибору транспорту:

> 📍 **Шлях до файлу:** `CargoOrderingService/src/CargoOrderingService/Extensions/Services/MassTransitServiceExtension.cs`  
> 📝 *У файлі наведено докладні покрокові коментарі українською мовою.*

[👉 **Перейти до файлу (клікніть тут)**](../CargoOrderingService/src/CargoOrderingService/Extensions/Services/MassTransitServiceExtension.cs#L20-L60)

```csharp
// 1. Якщо передано рядок підключення від Aspire (GetConnectionString("rabbitmq"))
if (!string.IsNullOrEmpty(rmqConnectionString))
{
    services.AddMassTransit(mt =>
    {
        mt.AddConsumers(Assembly.GetExecutingAssembly());
        mt.UsingRabbitMq((context, cfg) => { /* Налаштування підключення та ендпоінтів */ });
    });
}
// 2. Якщо брокер не налаштований (локальні тести без Docker) — Fallback на швидку пам'ять
else
{
    services.AddMassTransit(mt =>
    {
        mt.AddConsumers(Assembly.GetExecutingAssembly());
        mt.UsingInMemory((context, cfg) => { cfg.ConfigureEndpoints(context); });
    });
}
```

> 📍 **Шлях до файлу:** `DriversManagement/src/DriversManagement/Extensions/Services/MassTransitServiceExtension.cs`  
> 📝 *У файлі наведено докладні покрокові коментарі українською мовою.*

[👉 **Перейти до файлу (клікніть тут)**](../DriversManagement/src/DriversManagement/Extensions/Services/MassTransitServiceExtension.cs#L20-L60)

---

## 8. Порівняльна таблиця трьох комунікаційних парадигм у проєкті

| Характеристика | 1. HTTP Gateway Aggregator ([Гайд 02](02_Patterns_Synchronous_HttpClient_Aggregator_BFF.md)) | 2. gRPC RPC-зв'язок ([Гайд 03](03_Patterns_Synchronous_gRPC_Communication.md)) | 3. MassTransit + RabbitMQ ([Гайд 04](04_Patterns_Asynchronous_Messaging_MassTransit_RabbitMQ.md)) |
| :--- | :--- | :--- | :--- |
| **Тип взаємодії** | Синхронний Request/Response (Опитування багатьох API) | Синхронний Point-to-Point (Прямий бінарний дзвінок) | Асинхронний Publish/Subscribe (Подієво-орієнтований) |
| **Транспортний протокол** | HTTP/1.1 або HTTP/2 + JSON | HTTP/2 + Protocol Buffers | AMQP 0-9-1 через TCP сокет |
| **Часова залежність** | Жорстка: клієнт очікує збирання даних | Жорстка: обидва сервіси мають бути онлайн | **Повна відсутність (Temporal Decoupling)** |
| **Формат повідомлень** | Текстовий JSON | Бінарний Protobuf | JSON або BSON конверт MassTransit |
| **Основна сфера застосування** | Backend for Frontend (BFF) для UI/WPF/Web | Міжсервісні надшвидкі операції (RPC) | Реплікація даних, доменні події, саги |
| **Що стається при збої адресата?** | Часткова деградація (Graceful Degradation) | Помилка `RpcException` / Fallback значення | **Повідомлення надійно зберігається в черзі брокера** |

---

## 9. Чекліст найкращих практик (Production Best Practices)

- [x] **Контракти базуються на інтерфейсах:** Дозволяє поліморфну маршрутизацію та усуває пряму прив'язку до конкретних класів.
- [x] **Контракти зберігаються в SharedKernel:** Жоден мікросервіс не має прямого посилання на бізнес-збірки іншого.
- [x] **Сувора ідемпотентність у споживачах:** Кожен метод `Consume()` готовий до повторного отримання повідомлення (семантика *At-least-once delivery*).
- [x] **Кворум-черги (Quorum Queues) для критичних даних:** Реплікація на базі Raft-консенсусу захищає від збоїв кластера.
- [x] **Lazy Queues для оптимізації ресурсів:** Запобігає перевантаженню оперативної пам'яті брокера.
- [x] **In-Memory Fallback для автоматизованих тестів:** Юніт- та компонентні тести проходять миттєво в пам'яті без необхідності підіймати важкий контейнер RabbitMQ.

---

## 10. Швидка навігація по вихідному коду

* 📦 **Контракти повідомлень (SharedKernel):**
  * [👉 **Перейти до DriverCreated.cs**](../SharedKernel/DriverCreated.cs#L26-L52)
  * [👉 **Перейти до DriverUpdated.cs**](../SharedKernel/DriverUpdated.cs#L20-L36)
  * [👉 **Перейти до OrderMessages.cs**](../SharedKernel/OrderMessages.cs#L20-L40)
* 🚀 **Продюсери подій (Publishers):**
  * [👉 **Перейти до AddDriver.cs (DriversManagement)**](../DriversManagement/src/DriversManagement/Domain/Drivers/Features/AddDriver.cs#L45-L65)
  * [👉 **Перейти до UpdateDriver.cs (DriversManagement)**](../DriversManagement/src/DriversManagement/Domain/Drivers/Features/UpdateDriver.cs#L45-L65)
  * [👉 **Перейти до DriverCreatedEndpointRegistration.cs**](../DriversManagement/src/DriversManagement/Extensions/Services/ProducerRegistrations/DriverCreatedEndpointRegistration.cs#L8-L16)
  * [👉 **Перейти до DriverUpdatedEndpointRegistration.cs**](../DriversManagement/src/DriversManagement/Extensions/Services/ProducerRegistrations/DriverUpdatedEndpointRegistration.cs#L8-L16)
  * [👉 **Перейти до AddOrder.cs (CargoOrderingService)**](../CargoOrderingService/src/CargoOrderingService/Domain/Orders/Features/AddOrder.cs#L45-L60)
* 📥 **Споживачі (Consumers):**
  * [👉 **Перейти до SyncDriver.cs (DriversBlogManagement)**](../DriversBlogManagement/src/DriversBlogManagement/Domain/SyncDriver.cs#L50-L80)
  * [👉 **Перейти до SyncPatientEndpointRegistration.cs**](../DriversBlogManagement/src/DriversBlogManagement/Extensions/Services/ConsumerRegistrations/SyncPatientEndpointRegistration.cs#L10-L35)
  * [👉 **Перейти до SyncDriverInOrdering.cs (CargoOrderingService)**](../CargoOrderingService/src/CargoOrderingService/Domain/Drivers/Consumers/SyncDriverInOrdering.cs#L50-L80)
  * [👉 **Перейти до SyncDriverInOrderingEndpointRegistration.cs**](../CargoOrderingService/src/CargoOrderingService/Extensions/Services/ConsumerRegistrations/SyncDriverInOrderingEndpointRegistration.cs#L10-L30)
* ⚙️ **Конфігурація шини MassTransit:**
  * [👉 **Перейти до MassTransitServiceExtension.cs (Ordering)**](../CargoOrderingService/src/CargoOrderingService/Extensions/Services/MassTransitServiceExtension.cs#L20-L60)
  * [👉 **Перейти до MassTransitServiceExtension.cs (Drivers)**](../DriversManagement/src/DriversManagement/Extensions/Services/MassTransitServiceExtension.cs#L20-L60)

