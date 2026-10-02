# 💼 06. Патерн Unit of Work (Одиниця Роботи та Управління Транзакціями)

> **Модуль:** `01-Ntier-Drivers`  
> **Рівні реалізації:** `Business Logic (BLL)` ➔ `Data Access (Drivers.DAL.EF & Drivers.DAL.ADO)` ➔ `MS SQL Server`  
> **Ключові технології:** C# 13 • .NET 10 • Entity Framework Core Change Tracker • `IDbTransaction` (ADO.NET) • ACID Транзакції

---

## 📑 Зміст посібника

1. [Теоретична преамбула: Що це таке «на пальцях»](#1-теоретична-преамбула-що-це-таке-на-пальцях)
2. [Яку катастрофу лікує Unit of Work](#2-яку-катастрофу-лікує-unit-of-work)
   - 2.1. [Проблема часткових записів (Data Inconsistency)](#21-проблема-часткових-записів-data-inconsistency)
   - 2.2. [Проблема множинних підключень до БД](#22-проблема-множинних-підключень-до-бд)
3. [Архітектура Unit of Work у проєкті CargoDeliveryService](#3-архітектура-unit-of-work-у-проєкті-cargodeliveryservice)
   - 3.1. [Діаграма координації репозиторіїв](#31-діаграма-координації-репозиторіїв)
   - 3.2. [Контракт IEFUnitOfWork](#32-контракт-iefunitofwork)
   - 3.3. [Реалізація EFUnitOfWork (EF Core)](#33-реалізація-efunitofwork-ef-core)
4. [Глибоке порівняння: Unit of Work в EF Core проти ADO.NET / Dapper](#4-глибоке-порівняння-unit-of-work-в-ef-core-проти-adonet--dapper)
   - 4.1. [Механіка EF Core: Change Tracker та SaveChangesAsync](#41-механіка-ef-core-change-tracker-та-savechangesasync)
   - 4.2. [Механіка ADO.NET / Dapper: Явний IDbTransaction та Rollback](#42-механіка-adonet--dapper-явний-idbtransaction-та-rollback)
5. [Реальний бізнес-сценарій у BLL (Атомарний допуск водія до рейсу)](#5-реальний-бізнес-сценарій-у-bll-атомарний-допуск-водія-до-рейсу)
6. [Порівняльна таблиця підходів](#6-порівняльна-таблиця-підходів)
7. [Прямі посилання на файли коду проєкту](#7-прямі-посилання-на-файли-коду-проєкту)

---

## 1. Теоретична преамбула: Що це таке «на пальцях»

### 1.1. Життєва аналогія: Банківський переказ або посадка на літак
Уявіть ситуацію: ви купуєте квиток на літак і бронюєте готель у пакетному турі.
Операція складається з трьох дій:
1. Списати 15 000 грн з вашої банківської картки.
2. Забронювати конкретне місце 12A в літаку.
3. Забронювати номер у готелі.

Уявіть, що кроки 1 і 2 пройшли успішно, а на кроці 3 в готелі вимкнули світло і сталася помилка.
- **Без Unit of Work:** Гроші списалися, квиток на літак є, але готелю немає. Ви опиняєтесь на вулиці в чужій країні. Дані системи опинилися в «розірваному», невалідному стані.
- **З патерном Unit of Work:** Усі три дії об'єднуються в єдиний неподільний пакет (**Транзакцію**). Якщо готель забронювати не вдалося — спрацьовує **Rollback (Відкат)**: бронювання квитка скасовується, а гроші повертаються на картку! Або виконується **ВСЕ**, або не виконується **НІЧОГО**.

> 💡 **Визначення:** **Unit of Work (Одиниця Роботи)** — це патерн проєктування, який підтримує список бізнес-об'єктів, модифікованих під час транзакції, координує запис цих змін у базу даних і вирішує проблеми паралелізму, гарантуючи виконання принципу **ACID (Atomicity, Consistency, Isolation, Durability)**.

---

## 2. Яку катастрофу лікує Unit of Work

### 2.1. Проблема часткових записів (Data Inconsistency)
Розглянемо бізнес-логіку автопарку вантажних перевезень. Коли ми наймаємо водія, ми одночасно:
1. Створюємо запис водія в таблиці `Drivers`.
2. Завантажуємо його фотографію в `Photos`.
3. Виділяємо авансові витрати на паливо в таблиці `Expenses`.

Якщо кожен репозиторій сам викликає `SaveChanges()`:
```csharp
// ❌ АНТИПАТЕРН: Кожен репозиторій самостійно робить коміт у базу
public async Task RegisterDriverAsync(Driver driver, Photo photo, Expense expense)
{
    await _driverRepo.AddAsync(driver);     // SaveChanges() №1 -> УСПІХ в БД
    await _photoRepo.AddAsync(photo);       // SaveChanges() №2 -> УСПІХ в БД
    await _expenseRepo.AddAsync(expense);   // SaveChanges() №3 -> 💥 ЗБІЙ МЕРЕЖІ / ПОМИЛКА FK!
}
```
**Наслідок:** Водій та фото вже в базі, але запис витрат впав. База даних містить пошкоджені, «осиротілі» записи, які порушують правила комплаєнсу компанії.

### 2.2. Проблема множинних підключень до БД
Якщо у вас три репозиторії, кожен із яких відкриває власне з'єднання до бази даних і відправляє окремий `COMMIT TRANSACTION`, база даних витрачає втричі більше часу на перемикання контексту, блокування таблиць і мережеві затримки.

---

## 3. Архітектура Unit of Work у проєкті CargoDeliveryService

### 3.1. Діаграма координації репозиторіїв

```mermaid
flowchart TD
    Client["Клієнтський запит (API / BLL)"]
    UOW["Unit of Work (Координатор)"]
    
    subgraph Repositories ["Інкапсульовані репозиторії"]
        R1["EFDriverRepository"]
        R2["EFCompanyRepository"]
        R3["EFExpenseRepository"]
        R4["EFTruckRepository"]
    end
    
    Context["Єдиний контекст: DriversManagementContext"]
    DB[(MS SQL Server 2022)]

    Client -->|Викликає бізнес-метод| UOW
    UOW --> R1
    UOW --> R2
    UOW --> R3
    UOW --> R4

    R1 -.->|Фіксує сутність| Context
    R2 -.->|Фіксує сутність| Context
    R3 -.->|Фіксує сутність| Context
    R4 -.->|Фіксує сутність| Context

    UOW -->|Один атомарний виклик:\nCompleteAsync()| Context
    Context -->|Єдиний SQL-пакет INSERT/UPDATE\nв рамках транзакції| DB
```

---

### 3.2. Контракт `IEFUnitOfWork`
👉 Код: [`IEFUnitOfWork.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.DAL.EF/UOW/Contracts/IEFUnitOfWork.cs)

Контракт об'єднує всі спеціалізовані репозиторії та надає методи контролю транзакцій:

```csharp
public interface IEFUnitOfWork : IDisposable
{
    // Доступ до репозиторіїв домену
    IEFDriverRepository EFDriverRepository { get; }
    IEFCompanyRepository EFCompanyRepository { get; }
    IEFExpenseRepository EFExpenseRepository { get; }
    IEFInspectionRepository EFInspectionRepository { get; }
    IEFPhotoRepository EFPhotoRepository { get; }
    IEFRepairRepository EFPRepairRepository { get; }
    IEFTruckRepository EFPTruckRepository { get; }

    // Явне керування транзакцією (для складних операцій)
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    // Атомарна фіксація всіх накопичених змін
    Task<int> CompleteAsync(CancellationToken cancellationToken = default);
}
```

---

### 3.3. Реалізація `EFUnitOfWork` (EF Core)
👉 Код: [`EFUnitOfWork.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.DAL.EF/UOW/EFUnitOfWork.cs)

Усі репозиторії отримують **один і той самий екземпляр `DriversManagementContext`** через Dependency Injection (Scoped Lifecycle):

```csharp
public class EFUnitOfWork : IEFUnitOfWork
{
    private readonly DriversManagementContext databaseContext;
    private IDbContextTransaction? transaction;

    public IEFDriverRepository EFDriverRepository { get; }
    public IEFCompanyRepository EFCompanyRepository { get; }
    public IEFExpenseRepository EFExpenseRepository { get; }
    public IEFInspectionRepository EFInspectionRepository { get; }
    public IEFPhotoRepository EFPhotoRepository { get; }
    public IEFRepairRepository EFPRepairRepository { get; }
    public IEFTruckRepository EFPTruckRepository { get; }

    public EFUnitOfWork(
        DriversManagementContext databaseContext,
        IEFDriverRepository driverRepo,
        IEFCompanyRepository companyRepo,
        IEFExpenseRepository expenseRepo,
        IEFInspectionRepository inspectionRepo,
        IEFPhotoRepository photoRepo,
        IEFRepairRepository repairRepo,
        IEFTruckRepository truckRepo)
    {
        this.databaseContext = databaseContext;
        EFDriverRepository = driverRepo;
        EFCompanyRepository = companyRepo;
        EFExpenseRepository = expenseRepo;
        EFInspectionRepository = inspectionRepo;
        EFPhotoRepository = photoRepo;
        EFPRepairRepository = repairRepo;
        EFPTruckRepository = truckRepo;
    }

    // Атомарне збереження через Change Tracker
    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
    {
        return await databaseContext.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        transaction = await databaseContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (transaction != null) await transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (transaction != null) await transaction.RollbackAsync(cancellationToken);
    }

    public void Dispose()
    {
        transaction?.Dispose();
        databaseContext.Dispose();
    }
}
```

---

## 4. Глибоке порівняння: Unit of Work в EF Core проти ADO.NET / Dapper

У нашому рішенні реалізовано обидва підходи, що дає змогу наочно побачити фундаментальну різницю між ними:

### 4.1. Механіка EF Core: Change Tracker та `SaveChangesAsync`
* В Entity Framework Core сам `DbContext` уже є внутрішнім Unit of Work.
* Коли репозиторій викликає `table.AddAsync(entity)`, запис насправді **НЕ йде в базу даних**. Він просто помічається у внутрішньому реєстрі станів (`EntityState.Added`).
* Лише при виклику `CompleteAsync()` EF Core відкриває транзакцію, генерує оптимальний batch із SQL `INSERT/UPDATE` команд, виконує їх і комітить транзакцію.

### 4.2. Механіка ADO.NET / Dapper: Явний `IDbTransaction` та `Rollback`
👉 Код: [`UnitOfWork.cs (ADO)`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.DAL.ADO/UOW/UnitOfWork.cs)

У низькорівневому ADO.NET немає магічного Change Tracker. Тому Unit of Work бере під контроль явну транзакцію бази даних `IDbTransaction`:

```csharp
public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly IDbTransaction _dbTransaction;

    public UnitOfWork(IDbTransaction dbTransaction, ...)
    {
        _dbTransaction = dbTransaction;
        // Усі Dapper-репозиторії отримують цей самий екземпляр _dbTransaction!
    }

    public void Commit()
    {
        try
        {
            _dbTransaction.Commit();
        }
        catch (Exception)
        {
            _dbTransaction.Rollback(); // Миттєвий відкат при помилці!
            throw;
        }
    }
}
```

---

## 5. Реальний бізнес-сценарій у BLL (Атомарний допуск водія до рейсу)

Подивіться, як красиво і надійно виглядає використання `IEFUnitOfWork` у бізнес-сервісі:

```csharp
public async Task<bool> DispatchDriverToTripAsync(int driverId, int truckId, decimal fuelAdvance)
{
    // 1. Починаємо транзакцію
    await _unitOfWork.BeginTransactionAsync();

    try
    {
        // 2. Отримуємо сутності через різні репозиторії
        var driver = await _unitOfWork.EFDriverRepository.GetByIdAsync(driverId);
        var truck = await _unitOfWork.EFPTruckRepository.GetByIdAsync(truckId);

        // 3. Змінюємо стан сутностей (бізнес-правила)
        driver.EmploymentStatus = "OnRoute";
        truck.IsAvailable = false;

        // 4. Додаємо витрати
        await _unitOfWork.EFExpenseRepository.AddAsync(new EFExpense
        {
            DriverId = driverId,
            TruckId = truckId,
            ExpenseType = "FuelAdvance",
            Amount = fuelAdvance,
            Date = DateTime.UtcNow
        });

        // 5. Оновлюємо стан у репозиторіях
        await _unitOfWork.EFDriverRepository.UpdateAsync(driver);
        await _unitOfWork.EFPTruckRepository.UpdateAsync(truck);

        // 6. Атомарно зберігаємо все та фіксуємо транзакцію
        await _unitOfWork.CompleteAsync();
        await _unitOfWork.CommitTransactionAsync();

        return true;
    }
    catch (Exception)
    {
        // 7. Будь-який збій автоматично скасовує всі зміни!
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

---

## 6. Порівняльна таблиця підходів

| Критерій | Прямі виклики репозиторіїв | Unit of Work (Наш вибір) |
| :--- | :--- | :--- |
| **Гарантія транзакційності (ACID)** | ❌ Немає (кожен пише окремо) | ✅ Повна (або все, або нічого) |
| **Кількість підключень до БД** | $N$ відкриттів з'єднань | 1 спільне з'єднання на запит |
| **Мережеві затримки (Roundtrips)** | Численні запити туди-сюди | Пакетне виконання (Batching) |
| **Управління життєвим циклом (Scoped)** | Складно синхронізувати | Єдиний DI-контейнер гарантує 1 контекст на HTTP-запит |

---

## 7. Прямі посилання на файли коду проєкту

| Файл | Опис ролі в архітектурі |
| :--- | :--- |
| 📄 [`IEFUnitOfWork.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.DAL.EF/UOW/Contracts/IEFUnitOfWork.cs) | Контракт Unit of Work для Entity Framework Core |
| 📄 [`EFUnitOfWork.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.DAL.EF/UOW/EFUnitOfWork.cs) | Реалізація Unit of Work з підтримкою транзакцій та SaveChangesAsync |
| 📄 [`IUnitOfWork.cs (ADO)`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.DAL.ADO/UOW/Contracts/IUnitOfWork.cs) | Контракт Unit of Work для стеків ADO.NET / Dapper |
| 📄 [`UnitOfWork.cs (ADO)`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.DAL.ADO/UOW/UnitOfWork.cs) | Реалізація Unit of Work для Dapper з прямим контролем `IDbTransaction` |
| 📄 [`DriversManager.cs`](file:///c:/Users/Yurii/source/repos/Demo-CargoDeliveryService/01-Ntier-Drivers/Drivers.BLL/Managers/DriversManager.cs) | Бізнес-менеджер BLL, що використовує Unit of Work для складних сценаріїв |
