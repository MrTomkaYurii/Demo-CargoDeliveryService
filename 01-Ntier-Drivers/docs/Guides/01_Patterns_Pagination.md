# 📄 01. Патерн Пагінації (Pagination & Metadata Envelope Pattern)

> **Модуль:** `01-Ntier-Drivers`  
> **Рівні реалізації:** `Presentation (API/WPF)` ➔ `Business Logic (BLL)` ➔ `Data Access (DAL.EF & DAL.ADO)`  
> **Ключові технології:** C# 13 • .NET 10 • `IQueryable` • SQL `OFFSET / FETCH` • Metadata Envelope • AutoMapper

---

## 📑 Зміст посібника

1. [Теоретична преамбула (Суть і «Навіщо це потрібно?»)](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/docs/Guides/01_Patterns_Pagination.md#1-теоретична-преамбула-суть-і-навіщо-це-потрібно)
2. [Наскрізний потік виконання (End-to-End Architectural Flow)](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/docs/Guides/01_Patterns_Pagination.md#2-наскрізний-потік-виконання-end-to-end-architectural-flow)
3. [Розбір класів та коду проєкту по кроках](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/docs/Guides/01_Patterns_Pagination.md#3-розбір-класів-та-коду-проєкту-по-кроках)
4. [Підсумок та переваги архітектури](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/docs/Guides/01_Patterns_Pagination.md#4-підсумок-та-переваги-архітектури)
5. [Прямі посилання на файли коду проєкту](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/docs/Guides/01_Patterns_Pagination.md#5-прямі-посилання-на-файли-коду-проєкту)

---

## 1. Теоретична преамбула (Суть і «Навіщо це потрібно?»)

### 1.1. Що це таке простими словами (Життєва аналогія)
Уявіть велику телефонну книгу мегаполісу на 1 000 000 номерів. Якщо кур'єр запитає: *«Покажіть мені всі контакти»*, і ви вивалите на стіл 500 важких томів паперу, стіл зламається, а кур'єр не зможе знайти потрібне. 

Замість цього оператор відкриває **конкретну сторінку 15** і дає **20 номерів**, додаючи закладку: *«Сторінка 15 із 50 000, всього 1 000 000 номерів»*. Це і є **Пагінація з конвертом метаданих (Metadata Envelope)**.

### 1.2. Яку конкретну проблему це вирішує?
У наївній реалізації контролер виконує:
```csharp
// ❌ ПОГАНО: Завантажує всю базу в RAM сервера
[HttpGet]
public async Task<IActionResult> GetAll() => Ok(await _context.Drivers.ToListAsync());
```
Коли кількість водіїв зростає з 50 до 100 000:
1. **Вичерпання оперативної пам'яті (OutOfMemoryException):** Сервер виділяє сотні мегабайт пам'яті на кожен HTTP-запит.
2. **Мережеве перевантаження:** Сервер передає по мережі гігабайти нечитабельного JSON.
3. **Зависання клієнта:** WPF-додаток або браузер зависає на кілька секунд під час рендерингу 100 000 елементів у ListBox.
4. **Блокування бази даних:** Тривалі важкі операції `SELECT *` блокують таблиці або перевантажують пул з'єднань.

### 1.3. Порівняльна таблиця підходів

| Критерій | Наївний підхід (`GetAll`) | Патерн Пагінації у проєкті |
| :--- | :--- | :--- |
| **Навантаження на RAM** | Пропорційне розміру таблиці ($O(N)$) | Фіксоване ($O(PageSize)$, наприклад 10–50 об'єктів) |
| **Генерація SQL** | `SELECT * FROM Drivers` | `SELECT ... OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY` |
| **Захист від DoS** | Відсутній (клієнт може завалити сервер) | Жорсткий ліміт `PageSize <= 50` у валідаторі |
| **Метадані навігації** | Відсутні | Надає `TotalPages`, `TotalCount`, `HasNext`, `HasPrevious` |
| **UI-інтеграція** | Клієнт показує безкінечне «колесо завантаження» | Миттєвий рендеринг кнопок перегортання `<<`, `1`, `2`, `>>` |

---

## 2. Наскрізний потік виконання (End-to-End Architectural Flow)

```mermaid
sequenceDiagram
    autonumber
    actor Client as WPF Клієнт / Swagger
    participant Controller as DriversController (API)
    participant Validator as DriverParametersValidator (BLL)
    participant Manager as DriversManager (BLL)
    participant DAL as EFGenericRepository / EFDriverRepository (DAL)
    participant DB as MS SQL Server 2022

    Client->>Controller: GET /api/drivers?PageNumber=2&PageSize=10&MinYearOfBirth=1980
    Controller->>Validator: Перевірка DriverParameters
    Note over Validator: PageNumber >= 1, PageSize in [1..50], MaxYear >= MinYear
    Validator-->>Controller: Валідація успішна (ModelState.IsValid)
    
    Controller->>Manager: GetPaginatedDrivers(DriverParameters, provider, ct)
    Manager->>DAL: GetPaginatedDriversAsync(DriverParameters, ct)
    
    DAL->>DB: 1. SELECT COUNT(*) FROM Drivers WHERE ...
    DB-->>DAL: TotalCount = 120
    
    DAL->>DB: 2. SELECT ... OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY
    DB-->>DAL: 10 сутностей EFDriver
    
    Note over DAL: Створення PagedList<EFDriver> (обчислення TotalPages = 12)
    DAL-->>Manager: PagedList<EFDriver>
    
    Manager->>Manager: AutoMapper: PagedList<EFDriver> ➔ PagedResponseDTO<ShortDriverResponceDTO>
    Manager-->>Controller: PagedResponseDTO
    Controller-->>Client: HTTP 200 OK + JSON з Items та метаданими
```

---

## 3. Розбір класів та коду проєкту по кроках

Пагінація в системі складається з 4 взаємопов'язаних компонентів:

### 3.1. Базові параметри запиту: [`QueryStringParameters.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.DAL.EF/Entities/HelpModels/QueryStringParameters.cs)
Базовий клас, який гарантує, що клієнт не зможе запросити більше `maxPageSize = 50` записів:
```csharp
public abstract class QueryStringParameters
{
    const int maxPageSize = 50;
    public int PageNumber { get; set; } = 1;

    private int _pageSize = 10;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > maxPageSize ? maxPageSize : value;
    }

    public string OrderBy { get; set; }
}
```

### 3.2. Доменні параметри з фільтром: [`DriverParameters.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.DAL.EF/Entities/HelpModels/DriverParameters.cs)
Розширює базові параметри специфічними критеріями бізнесу (діапазон років народження водіїв):
```csharp
public class DriverParameters : QueryStringParameters
{
    public DriverParameters() => OrderBy = "LastName";

    public uint MinYearOfBirth { get; set; }
    public uint MaxYearOfBirth { get; set; } = (uint)DateTime.Now.Year;
    public string LastName { get; set; }
}
```

### 3.3. Ядро обчислення та колекція: [`PagedList.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.DAL.EF/Helpers/PagedList.cs)
Колекція `PagedList<T>`, яка спадкується від `List<T>`, розраховує метадані та виконує відкладену вибірку в БД через методи `ToPagedList` та асинхронний `ToPagedListAsync`:
```csharp
public class PagedList<T> : List<T>
{
    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }

    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;

    public static async Task<PagedList<T>> ToPagedListAsync(
        IQueryable<T> source, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var count = await source.CountAsync(cancellationToken); // Запит 1: COUNT
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken); // Запит 2: OFFSET/FETCH

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}
```

### 3.4. Клієнтська DTO-обгортка: [`PagedResponseDTO.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.BLL/DTOs/Responses/PagedResponseDTO.cs)
Чистий контракт для клієнтів (WPF, Mobile, SPA), який містить як самі елементи, так і навігаційні прапорці:
```csharp
public class PagedResponseDTO<T>
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasPrevious { get; set; }
    public bool HasNext { get; set; }
}
```

### 3.5. Обробка у контролері: [`DriversController.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/DriversApi/Controllers/DriversController.cs)
Контролер приймає параметри пагінації у query string та повертає запакований `PagedResponseDTO`:
```csharp
[HttpGet]
public async Task<ActionResult<PagedResponseDTO<ShortDriverResponceDTO>>> GetDrivers(
    [FromQuery] DriverParameters driverParameters,
    [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
    CancellationToken cancellationToken = default)
{
    var response = await _driversManager.GetPaginatedDrivers(driverParameters, provider, cancellationToken);
    return Ok(response);
}
```

---

## 4. Підсумок та переваги архітектури
Патерн пагінації гарантує масштабованість системи: незалежно від того, чи в базі даних 1 000 водіїв, чи 10 000 000, навантаження на пам'ять сервера залишається стабільним ($O(1)$ відносно розміру БД).

---

## 5. Прямі посилання на файли коду проєкту

| Файл коду | Шар архітектури | Призначення |
| :--- | :--- | :--- |
| 📄 [`QueryStringParameters.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.DAL.EF/Entities/HelpModels/QueryStringParameters.cs) | Data Access (DAL) | Базовий клас з обмеженням `maxPageSize = 50` та номером сторінки |
| 📄 [`DriverParameters.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.DAL.EF/Entities/HelpModels/DriverParameters.cs) | Data Access (DAL) | Доменні параметри фільтрації водіїв за роком народження та прізвищем |
| 📄 [`PagedList.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.DAL.EF/Helpers/PagedList.cs) | Data Access (DAL) | Фабричні методи розрахунку сторінок та виконання SQL `COUNT` + `OFFSET/FETCH` |
| 📄 [`PagedResponseDTO.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.BLL/DTOs/Responses/PagedResponseDTO.cs) | Business Logic (BLL) | Універсальний Metadata Envelope контракт для зовнішніх API-клієнтів |
| 📄 [`DriverParametersValidator.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.BLL/DTOs/Validation/DriverParametersValidator.cs) | Business Logic (BLL) | FluentValidation валідатор діапазонів сторінок та бізнес-років |
| 📄 [`DriversManager.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.BLL/Managers/DriversManager.cs) | Business Logic (BLL) | Координація репозиторіїв та мапінг `PagedList` у `PagedResponseDTO` |
| 📄 [`DriversController.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/DriversApi/Controllers/DriversController.cs) | Presentation (API) | REST Endpoint пагінації з підтримкою `CancellationToken` |
