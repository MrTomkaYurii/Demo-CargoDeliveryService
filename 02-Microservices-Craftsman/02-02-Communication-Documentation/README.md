# 📡 02-02-Communication-Documentation: Міжсервісна Комунікація та Патерни

> Комплексна технічна документація та посібники з архітектурних патернів, синхронної (gRPC) та асинхронної (MassTransit + RabbitMQ) взаємодії мікросервісів у блоці **02-Microservices-Craftsman**.

---

## 📚 Зміст посібників

### 1. 🏛️ [01_Patterns_Microservices_Clean_Architecture.md](01_Patterns_Microservices_Clean_Architecture.md)
* **Тематика:** Clean Architecture, Domain-Driven Design (DDD) та Vertical Slice Architecture.
* **Ключові концепції:**
  * Відокремлені Bounded Contexts для сутності `Driver` у різних сервісах.
  * Організація CQRS за допомогою MediatR (команди, запити, валідатори FluentValidation).
  * Порівняння з монолітним N-Tier підходом з Блоку 1.

### 2. ⚡ [02_Patterns_Synchronous_gRPC_Communication.md](02_Patterns_Synchronous_gRPC_Communication.md)
* **Тематика:** Синхронний високопродуктивний міжсервісний зв'язок на базі gRPC та HTTP/2.
* **Ключові концепції:**
  * Контракти Protobuf (`Driver.proto`) та компіляція типів.
  * Реалізація сервера `DriverRpcService` у `DriversBlogManagement`.
  * Типізований клієнт `DriverRpcClient` у `DriversManagement` з резолвінгом через Service Discovery.
  * Патерн стійкості Graceful Degradation при збої віддаленого RPC-сервісу.

### 3. 📬 [03_Patterns_Asynchronous_Messaging_MassTransit_RabbitMQ.md](03_Patterns_Asynchronous_Messaging_MassTransit_RabbitMQ.md)
* **Тематика:** Подієво-орієнтована архітектура (Event-Driven Architecture) та шина повідомлень MassTransit.
* **Ключові концепції:**
  * Патерн Publish/Subscribe та типові топології RabbitMQ (Fanout Exchanges).
  * Контракти доменних подій із `SharedKernel` (`IDriverCreated`, `IDriverUpdated`, `IOrderCreated`).
  * Споживачі (Consumers) у `DriversBlogManagement` та `CargoOrderingService`.
  * Наскрізний Distributed Tracing повідомлень через OpenTelemetry (OTLP).

---

## 📑 Огляди мікросервісів

* 📦 **[CargoOrderingService-README.md](CargoOrderingService-README.md)** — Специфікація мікросервісу замовлень, доставок та життєвого циклу рейсів.
* 📝 **[DriversBlogManagement-README.md](DriversBlogManagement-README.md)** — Специфікація мікросервісу блогу водіїв, коментарів, постів та gRPC-сервера.

---

## 🔗 Зв'язок з іншими розділами

* 🚀 **[.NET Aspire Оркестрація](../02-01-Aspire-Documentation/02-01-Aspire-MasterClass.md)** — Інтерактивний Майстер-Клас з хмарно-орієнтованої оркестрації, запуску контейнерів та єдиної OTLP телеметрії.
