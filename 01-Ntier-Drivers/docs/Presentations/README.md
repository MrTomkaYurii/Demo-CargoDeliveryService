# 📽️ Каталог Архітектурних Презентацій проєкту CargoDeliveryService

Ця директорія призначена для збереження **архітектурних презентацій**, слайдових майстер-класів у форматі Markdown (`.md`), а також презентацій у форматі PowerPoint (`.pptx` / `.pdf`), структурованих за модулями системи.

---

## 🗂️ Доступні презентації:

| № | Презентація | Формат | Модуль | Опис |
| :-: | :--- | :---: | :---: | :--- |
| **01** | [01-Ntier-3Tier-Architecture-Masterclass.md](./01-Ntier-3Tier-Architecture-Masterclass.md) | Markdown Slides (22 слайди) | `NtierDriversModule` | Інтерактивна презентація з навігацією: 3-Tier, Generic Repository, Unit of Work, Dual-DAL (EF vs Dapper), Specification Pattern, DTO, AutoMapper, FluentValidation, Exception Middleware, WPF MVVM. |

---

## 📌 Інструкція для додавання нових презентацій (Markdown / PPTX):
1. **Для нових модулів** (наприклад, `CargoOrderingService`, `DriversBlogManagement`, `KeycloakPulumiAuth`) створюйте файл презентації за шаблоном:
   - `02-CargoOrdering-CQRS-MediatR-Masterclass.md` (або відповідний `.pptx`).
2. Якщо ви додаєте класичний файл презентації PowerPoint (`.pptx`), зберігайте його безпосередньо в цій теці:
   - `docs/Presentations/01-Ntier-Architecture.pptx`
   - `docs/Presentations/02-CargoOrdering.pptx`
3. Усі презентації автоматично підв'язані у Solution Explorer Visual Studio в папці рішення `Documentation / Presentations`.
