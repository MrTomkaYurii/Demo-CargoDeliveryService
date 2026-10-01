# 🚛 01-Ntier-Drivers: Enterprise 3-Tier Monolith & WPF Client

> Корпоративна 3-рівнева система на базі **.NET 10** для управління водіями та автопарком вантажних перевезень. Демонструє паралельну підтримку **Entity Framework Core 10** та **Dapper / ADO.NET**, патерни **Specification Pattern**, **Generic Repository**, **Unit of Work**, валідацію **FluentValidation**, централізовану обробку помилок **RFC 7807** та десктопний клієнт **WPF (MVVM)** у фірмовому фіолетовому стилі.

---

## 📂 Структура проєктів блоку

| Проєкт | Рівень / Роль | Опис |
| :--- | :--- | :--- |
| **`WPFDrivers`** | Presentation (Desktop) | Десктопний клієнт WPF з патерном MVVM, стилізований у фіолетових корпоративних відтінках C# |
| **`DriversApi`** | Presentation (REST API) | ASP.NET Core Web API, Swagger з XML-документацією, авторедирект `/swagger` |
| **`Drivers.BLL`** | Business Logic (BLL) | Бізнес-менеджери (`DriversManager`, `TruckManager`, `CompanyManager`), правила комплаєнсу та скорингу |
| **`Drivers.DAL.EF`** | Data Access (ORM) | EF Core 10, Specification Pattern (`DriverFilterSpecification`), `SpecificationEvaluator` |
| **`Drivers.DAL.ADO`** | Data Access (Micro-ORM) | ADO.NET та Dapper з оптимізованими SQL-запитами та `UnitOfWork` |
| **`Drivers.Db`** | Database Schema | SQL Server Database Project (`.sqlproj`) зі схемами таблиць та обмежень |
| **`Drivers.JWT`** | Security Helpers | SQL Server Database Project для автентифікації водіїв |
| **`FakeDataDbGenerator`** | Data Seeder | Генератор тестових даних на базі бібліотеки Bogus |
| **`NtierDrivers.UnitTests`** | Quality Assurance | Комплекс юніт-тестів (49 тестів), що покривають специфікації, валідацію та бізнес-логіку |

---

## 📚 Документація та посібники блоку

### 🎓 Презентації:
- **[Презентація майстер-класу (22 слайди)](docs/Presentations/01-Ntier-3Tier-Architecture-Masterclass.md)** — повний інтерактивний конспект за стандартом "Аналогія ➔ Проблема ➔ Погано vs Добре ➔ Архітектура ➔ Код".

### 📘 Серія посібників з архітектурних патернів (Guides):
- 📄 **[01. Патерн Пагінації](docs/Guides/01_Patterns_Pagination.md)** — наскрізний потік `QueryStringParameters` ➔ `PagedList<T>` ➔ SQL `OFFSET/FETCH` ➔ `PagedResponseDTO<T>`.
- 🛡️ **[02. Патерн Валідації](docs/Guides/02_Patterns_Validation.md)** — порівняння Data Annotations / `ModelState` проти FluentValidation, крос-польова валідація, тести.
- 🚨 **[03. Патерн Централізованої Обробки Помилок](docs/Guides/03_Patterns_Centralized_Exception_Handling.md)** — `ExceptionMiddleware`, стандарт RFC 7807, безпека від витоку SQL, трейсинг через `ErrorId`.
- 📐 **[04. Specification, Repository & Unit of Work](docs/Guides/Specification-and-Repository-Guide.md)** — динамічна побудова LINQ Expression Trees через `SpecificationEvaluator`.
- ⚡ **[05. Порівняння EF Core 10 vs Dapper](docs/Guides/EF-vs-Dapper-Architecture-Guide.md)** — аналіз швидкодії ORM vs Micro-ORM, бенчмаркінг, Dual-Engine Strategy.

---

## 🚀 Запуск та перевірка

### 1. Запуск юніт-тестів
```powershell
dotnet test 01-Ntier-Drivers/NtierDrivers.UnitTests/NtierDrivers.UnitTests.csproj
```
*(Всі 49 тестів проходять успішно).*

### 2. Запуск DriversApi
```powershell
dotnet run --project 01-Ntier-Drivers/DriversApi/Drivers.Api.csproj
```
API автоматично відкриє Swagger UI за адресою: `http://localhost:5000/swagger`.
