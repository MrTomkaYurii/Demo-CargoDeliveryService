# 📚 02-01-Aspire-Documentation: Оркестрація та Розподілена Телеметрія

> Документаційний та навчальний модуль Блоку 2 (`02-Microservices-Craftsman`), присвячений впровадженню **.NET 10 Aspire Orchestration** поверх розподіленої мікросервісної підсистеми вантажних перевезень.

---

## 🎯 Матеріали модуля

| Документ | Опис | Формат |
| :--- | :--- | :--- |
| **[02-01-Aspire-MasterClass.md](02-01-Aspire-MasterClass.md)** | Повний 15-слайдовий майстер-клас "від новачка до профі" з детальним розбором C# AppHost, ServiceDefaults, OTLP, gRPC discovery, MassTransit tracing та покроковою інструкцією запуску | 🎬 Інтерактивні слайди з прямими переходами до коду |

---

## 🏗️ Архітектура розподіленої системи під .NET Aspire

```mermaid
flowchart TD
    subgraph Dashboard ["📊 .NET Aspire Dashboard"]
        D_UI["Real-time Traces • Structured Logs • Metrics • Health Probes"]
    end

    subgraph AppHost ["🚀 CargoDelivery.AppHost (Orchestrator)"]
        direction TB
        RMQ[("🐰 RabbitMQ Container<br/>(amqp:5672, mgmt:15672)")]
        SQL[("💾 MS SQL Server 2022<br/>(3 ізольовані БД)")]
        
        DM["🚚 DriversManagement (.NET 10)<br/>• Driver Aggregate Root<br/>• Gateway Aggregator (BFF)"]
        DB["📝 DriversBlogManagement (.NET 10)<br/>• Social Feed API<br/>• gRPC Server (HTTP/2)"]
        CO["📦 CargoOrderingService (.NET 10)<br/>• Logistics API<br/>• MassTransit Pub/Sub"]

        SQL -->|dev_driversmanagement| DM
        SQL -->|dev_driversblogmanagement| DB
        SQL -->|dev_cargoorderingservice| CO

        RMQ -.->|Event Bus| DM
        RMQ -.->|Event Bus| DB
        RMQ -.->|Event Bus| CO

        DM -- "1. gRPC Discovery (HTTP/2)" --> DB
        DM -- "2. HTTP Aggregator BFF (Task.WhenAll)" --> DB
        DM -- "3. HTTP Aggregator BFF (Task.WhenAll)" --> CO
    end

    AppHost -. "OTLP (OpenTelemetry Protocol)" .-> Dashboard
```

---

## ⚡ Швидкий старт для розробника

> 📍 **Шлях до файлу оркестратора:** `02-Microservices-Craftsman/Aspire/CargoDelivery.AppHost/AppHost.cs`

[👉 **Перейти до файлу (клікніть тут)**](../Aspire/CargoDelivery.AppHost/AppHost.cs#L1-L68)

```powershell
# Запуск усієї системи (БД + Брокер + 3 API + Дашборд) однією командою:
dotnet run --project "02-Microservices-Craftsman\Aspire\CargoDelivery.AppHost\CargoDelivery.AppHost.csproj"
```
