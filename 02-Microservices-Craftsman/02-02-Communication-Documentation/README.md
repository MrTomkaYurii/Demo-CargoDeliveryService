# 📡 02-02-Communication-Documentation: Міжсервісна Комунікація та Архітектурні Патерни

> Комплексна технічна документація та посібники з архітектурних патернів, синхронної HTTP-комунікації (Aggregator / BFF), високопродуктивного gRPC та асинхронного обміну через брокер повідомлень (MassTransit + RabbitMQ) у блоці **02-Microservices-Craftsman**.

---

## 📚 Логічна послідовність посібників (Pattern Guides)

Документація вибудувана у чіткій методичній послідовності — від архітектурного фундаменту до базової HTTP-комунікації, від бінарного RPC до повністю асинхронного брокера повідомлень.

> 🧭 **Почніть з карти:** [00_Communication_Matrix_Quick_Reference.md](00_Communication_Matrix_Quick_Reference.md) — усі три види комунікації на одній сторінці з навігаційними таблицями до конкретних рядків коду.
> 🔗 **Посилання на код** мають вигляд `Файл.cs#L42` — у VS Code (Markdown Preview, `Ctrl+Shift+V`) клік відкриває файл одразу на рядку, на GitHub рядок підсвічується.
> 🧪 **Усе перевірено на живому стенді** (`dotnet run --project Aspire/CargoDelivery.AppHost`); готові запити для кожного сценарію — у [CargoDelivery.HttpAggregator.http](../CargoDelivery.HttpAggregator/CargoDelivery.HttpAggregator.http).

### 1. 🏛️ [01_Patterns_Microservices_Clean_Architecture.md](01_Patterns_Microservices_Clean_Architecture.md)
* **Тематика:** Архітектурний фундамент: Clean Architecture, Domain-Driven Design (DDD) та Vertical Slice Architecture.
* **Ключові концепції:**
  * Відокремлені Bounded Contexts для сутності `Driver` у різних сервісах.
  * Організація CQRS за допомогою MediatR (команди, запити, шлях `POST /api/drivers` крізь код).
  * Пастки моделі Craftsman: спільний PK у зв'язках «один-до-одного», `ProjectReference` між сервісами.
  * Порівняння з монолітним N-Tier підходом з Блоку 1.

### 2. 🌐 [02_Patterns_Synchronous_HttpClient_Aggregator_BFF.md](02_Patterns_Synchronous_HttpClient_Aggregator_BFF.md)
* **Тематика:** Синхронна HTTP-взаємодія: Gateway Aggregator та Backend for Frontend (BFF).
* **Ключові концепції:**
  * Проблема надлишкових запитів клієнтів (Network Chattiness) та композиція API в один комплексний ендпоінт.
  * Використання `IHttpClientFactory` та типізованих клієнтів (Typed Clients) у .NET 10.
  * Паралельний Fan-Out / Fan-In (`Task.WhenAll`) для одночасного опитування підпорядкованих мікросервісів.
  * Стійкість та ізоляція збоїв: патерн Graceful Degradation (повернення часткових даних при недоступності окремого сервісу).
  * Нативне розпізнавання імен через .NET Aspire Service Discovery (`https+http://cargoorderingservice`, тощо).
  * Внутрішні (service-to-service) ендпоінти `api/internal/*` та окремий короткий бюджет Polly для некритичних залежностей.
  * Порівняння вбудованого BFF (`DriversManagement`) і виділеного агрегатора (`CargoDelivery.HttpAggregator`).

### 3. ⚡ [03_Patterns_Synchronous_gRPC_Communication.md](03_Patterns_Synchronous_gRPC_Communication.md)
* **Тематика:** Синхронний високопродуктивний міжсервісний зв'язок на базі gRPC та HTTP/2.
* **Ключові концепції:**
  * Чому для внутрішнього зв'язку сервіс-до-сервісу обирають бінарний gRPC замість текстового REST JSON.
  * Контракти Protobuf (`Driver.proto`) та автоматична MSBuild-кодогенерація типів C#.
  * Реалізація gRPC-сервера `DriverRpcService` у `DriversBlogManagement`.
  * Contract-first: клієнт `DriverRpcClient` у `DriversManagement` генерується з того самого `.proto` (`GrpcServices="Client"`), резолвінг через Service Discovery.
  * Статуси gRPC (`grpc-status` у трейлері), HTTP/2, wire-формат Protobuf, діагностика через трасування.
  * Патерн стійкості Graceful Degradation при збої віддаленого RPC-сервісу.

### 4. 📬 [04_Patterns_Asynchronous_Messaging_MassTransit_RabbitMQ.md](04_Patterns_Asynchronous_Messaging_MassTransit_RabbitMQ.md)
* **Тематика:** Подієво-орієнтована архітектура (Event-Driven Architecture) та шина повідомлень MassTransit.
* **Ключові концепції:**
  * Перехід від блокуючої синхронності до Temporal Decoupling: брокер як буфер надійності.
  * Патерн Publish/Subscribe та типові топології RabbitMQ (Fanout Exchanges & Quorum Queues; чому «lazy»-аргумент був міфом).
  * Повтори споживачів (`UseMessageRetry`), черги `_error`, Dual-Write і Transactional Outbox.
  * Контракти доменних подій із `SharedKernel` (`IDriverCreated`, `IDriverUpdated`, `IOrderCreated`).
  * Споживачі (Consumers) у `DriversBlogManagement` та `CargoOrderingService`.
  * Наскрізний Distributed Tracing повідомлень через OpenTelemetry (OTLP).

---

## 📑 Огляди мікросервісів

* 🚚 **[DriversManagement-README.md](DriversManagement-README.md)** — Головний сервіс обліку водіїв, автопарку, ремонтів, техоглядів, gRPC-клієнт та видавець подій.
* 📦 **[CargoOrderingService-README.md](CargoOrderingService-README.md)** — Специфікація мікросервісу замовлень, доставок, рейсів та обробки подій водіїв.
* 📝 **[DriversBlogManagement-README.md](DriversBlogManagement-README.md)** — Специфікація мікросервісу блогу водіїв, постів, коментарів та вбудованого gRPC-сервера.

---

## 🔗 Зв'язок з іншими розділами

* 🚀 **[.NET Aspire Оркестрація](../02-01-Aspire-Documentation/02-01-Aspire-MasterClass.md)** — Інтерактивний Майстер-Клас (15 слайдів) з хмарно-орієнтованої оркестрації, запуску контейнерів та єдиної OTLP телеметрії.
* 🛡️ **[Виділений Gateway Aggregator](../02-01-Aspire-Documentation/03_HttpAggregator_Gateway_Architecture_And_Design.md)** та **[Polly / Service Discovery](../02-01-Aspire-Documentation/06_ServiceDiscovery_Resilience_Polly_HttpClient.md)** — поглиблення до гайду 02.
* 📦 **[Головний README Блоку 2](../README.md)** — Загальна архітектура, матриця портів та запуск системи.
