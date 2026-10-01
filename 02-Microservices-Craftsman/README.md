# 📦 02-Microservices-Craftsman: Clean Architecture, CQRS & Microservices

> Розподілена мікросервісна підсистема вантажних перевезень, згенерована за допомогою інструменту **Craftsman** на базі принципів **Clean Architecture**, **Domain-Driven Design (DDD)** та **CQRS (MediatR)**.

---

## 📂 Структура проєктів блоку

| Мікросервіс / Проєкт | Опис | Архітектурний стек |
| :--- | :--- | :--- |
| **`CargoOrderingService`** | Сервіс керування замовленнями вантажів, доставками та відстеженням маршрутів | Clean Architecture, MediatR, FluentValidation, EF Core, Unit/Integration/Functional tests |
| **`DriversBlogManagement`** | Сервіс блогу водіїв, постів, коментарів, лайків та взаємодії користувачів | Clean Architecture, CQRS, MediatR, Unit/Integration/Functional tests |
| **`DriversManagement`** | Сервіс обліку водіїв, автопарку, компаній та технічного обслуговування | Clean Architecture, CQRS, MediatR, Shared Kernel інтеграція |
| **`SharedKernel`** | Спільне ядро подій та контрактів між мікросервісами (`DriverUpdated`) | Domain Events, Shared Value Objects, MassTransit Contracts |

---

## 📚 Документація мікросервісів

- 📘 **[CargoOrderingService Documentation](docs/CargoOrderingService-README.md)** — докладна документація по API, ендпоінтах та тестах замовлень.
- 📘 **[DriversBlogManagement Documentation](docs/DriversBlogManagement-README.md)** — документація сервісу блогу та взаємодії користувачів.

---

## 🛠️ Інфраструктура та план розвитку (Roadmap)

1. **Міжсервісна взаємодія через gRPC:**
   - Прямий високопродуктивний бінарний зв'язок між `CargoOrderingService` та `DriversManagement` для перевірки статусу водія.
2. **Асинхронний обмін подіями через Message Brokers (MassTransit + RabbitMQ):**
   - Публікація подій (наприклад, `DriverUpdated`, `OrderCreated`, `DeliveryAssigned`) через шину повідомлень.
3. **Розподілений трейсинг:**
   - Jaeger Tracing (порти 56391, 16686 у Docker Compose) та OpenTelemetry для спостережуваності транзакцій.
