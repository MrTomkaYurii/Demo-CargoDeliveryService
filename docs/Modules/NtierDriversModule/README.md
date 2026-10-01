# 🚚 Модуль Управління Водіями (NtierDriversModule)

## 📌 Огляд модуля
Модуль **`NtierDriversModule`** відповідає за облік водіїв, вантажівок, техоглядів, ремонтів та фінансових витрат логістичної компанії.

## 🏛️ Архітектурна будова
Модуль реалізований за чистою **Тришаровою архітектурою (3-Tier)**:
- **`DriversApi`**: REST Web API (Controllers, Swagger OpenAPI, RFC 7807 Exception Middleware).
- **`WPFDrivers`**: Десктопний клієнт (WPF, MVVM, Live Benchmarking UI).
- **`Drivers.BLL`**: Бізнес-менеджери, DTO, валідатори FluentValidation, AutoMapper, розрахункові калькулятори тарифів та KPI-скорингу.
- **`Drivers.DAL.EF`**: Рівень ORM на базі Entity Framework Core 10 (Specification Pattern, Change Tracker).
- **`Drivers.DAL.ADO`**: Рівень Micro-ORM на базі ADO.NET та Dapper (Raw SQL, пакетні запити `QueryMultipleAsync`).
- **`NtierDrivers.UnitTests`**: Тестове покриття (49 тестів / 100% успішність).

## 🔗 Посилання на документацію модуля:
- 🎓 [Архітектурна презентація (Майстер-клас)](../../Presentations/01-Ntier-3Tier-Architecture-Masterclass.md)
- 📘 [Технічний довідник: Specification, Repository & Unit of Work](../../Guides/Specification-and-Repository-Guide.md)
- ⚡ [Порівняльний аналіз: EF Core проти Dapper](../../Guides/EF-vs-Dapper-Architecture-Guide.md)
