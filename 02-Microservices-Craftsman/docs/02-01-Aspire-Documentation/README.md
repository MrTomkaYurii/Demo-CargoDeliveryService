# 📚 02-01-Aspire-Documentation: Оркестрація та Розподілена Телеметрія

> Документаційний та навчальний модуль Блоку 2 (`02-Microservices-Craftsman`), присвячений впровадженню **.NET Aspire 13** поверх розподіленої мікросервісної підсистеми вантажних перевезень.

---

## 🎯 Матеріали модуля

| Документ | Опис | Формат |
| :--- | :--- | :--- |
| **[02-01-Aspire-MasterClass.md](02-01-Aspire-MasterClass.md)** | Повний 15-слайдовий майстер-клас "від чайника до профі" з розбором архітектури, AppHost, ServiceDefaults, OTLP, gRPC discovery, MassTransit tracing та покроковою інструкцією запуску | 🎬 Інтерактивні слайди з посиланнями на код |

---

## 🏗️ Архітектура обгортки .NET Aspire

```mermaid
flowchart TD
    subgraph Dashboard ["📊 .NET Aspire Dashboard"]
        D_UI["Real-time Traces • Logs • Metrics • Health"]
    end

    subgraph AppHost ["🚀 CargoDelivery.AppHost"]
        direction TB
        RMQ[("🐰 RabbitMQ Container")]
        SQL[("💾 SQL Server Container")]
        
        DM["🚚 DriversManagement (.NET 10)"]
        DB["📝 DriversBlogManagement (.NET 10)"]
        CO["📦 CargoOrderingService (.NET 10)"]

        RMQ -.-> DM
        RMQ -.-> DB
        RMQ -.-> CO

        SQL -.-> DM
        SQL -.-> DB
        SQL -.-> CO

        DM -- "gRPC Discovery" --> DB
    end

    AppHost -. "OTLP (OpenTelemetry)" .-> Dashboard
```

---

## ⚡ Швидкий старт

```powershell
# Запуск усієї системи (БД + Брокер + 3 API + Дашборд) однією командою:
dotnet run --project "02-Microservices-Craftsman\Aspire\CargoDelivery.AppHost\CargoDelivery.AppHost.csproj"
```
