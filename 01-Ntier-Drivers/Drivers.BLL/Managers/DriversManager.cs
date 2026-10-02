using System.Diagnostics;
using AutoMapper;
using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Exceptions;
using Drivers.BLL.Managers.Contracts;
using Drivers.BLL.Models;
using Drivers.DAL.ADO.UOW.Contracts;
using Drivers.DAL.EF.Entities;
using Drivers.DAL.EF.Entities.HelpModels;
using Drivers.DAL.EF.UOW.Contracts;
using Microsoft.Extensions.Logging;
using ADODriver = Drivers.DAL.ADO.Entities.Driver;
using ADODriverParams = Drivers.DAL.ADO.Entities.HelpModels.DriverParameters;

namespace Drivers.BLL.Managers;

/// <summary>
/// Менеджер водіїв. Реалізує бізнес-правила, виклики репозиторіїв через обраний DataAccessProvider,
/// автоматичне зіставлення в DTO та точне порівняння продуктивності EF Core проти ADO.NET / Dapper.
/// </summary>
public class DriversManager : IDriversManager
{
    private readonly ILogger<DriversManager> _logger;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _adoUow;
    private readonly IEFUnitOfWork _efUow;

    public DriversManager(
        ILogger<DriversManager> logger,
        IMapper mapper,
        IUnitOfWork adoUow,
        IEFUnitOfWork efUow)
    {
        _logger = logger;
        _mapper = mapper;
        _adoUow = adoUow;
        _efUow = efUow;
    }

    /// <summary>
    /// Отримання повної інформації про водія через обраний провайдер
    /// </summary>
    public async Task<FullDriverResponceDTO> GetFullInfoAboutDriver(
        int id,
        DataAccessProvider provider,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Отримання повної інформації про водія {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var driver = await _adoUow.DriverRepository.GetCompleteEntityAsync(id);
            if (driver == null)
            {
                throw new NotFoundException($"Водія з Id {id} не знайдено в базі даних (Dapper).");
            }
            return _mapper.Map<FullDriverResponceDTO>(driver);
        }
        else
        {
            var driver = await _efUow.EFDriverRepository.GetCompleteEntityAsync(id, cancellationToken);
            if (driver == null)
            {
                throw new NotFoundException($"Водія з Id {id} не знайдено в базі даних (EF Core).");
            }
            return _mapper.Map<FullDriverResponceDTO>(driver);
        }
    }

    /// <summary>
    /// Отримання списку всіх водіїв у скороченому форматі
    /// </summary>
    public async Task<IEnumerable<ShortDriverResponceDTO>> GetListOfAllDrivers(
        DataAccessProvider provider,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Отримання списку всіх водіїв через {Provider}", provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var drivers = await _adoUow.DriverRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ShortDriverResponceDTO>>(drivers);
        }
        else
        {
            var drivers = await _efUow.EFDriverRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<ShortDriverResponceDTO>>(drivers);
        }
    }

    /// <summary>
    /// Пагінований список водіїв із фільтрацією та сортуванням
    /// </summary>
    public async Task<PagedResponseDTO<ShortDriverResponceDTO>> GetPaginatedDrivers(
        DriverParameters driverParameters,
        DataAccessProvider provider,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Отримання пагінованого списку водіїв через {Provider}. Сторінка {Page}, Розмір {Size}",
            provider, driverParameters.PageNumber, driverParameters.PageSize);

        if (provider == DataAccessProvider.Dapper)
        {
            var adoParams = new ADODriverParams
            {
                PageNumber = driverParameters.PageNumber,
                PageSize = driverParameters.PageSize,
                OrderBy = driverParameters.OrderBy,
                MinYearOfBirth = driverParameters.MinYearOfBirth,
                MaxYearOfBirth = driverParameters.MaxYearOfBirth,
                LastName = driverParameters.LastName
            };

            var pagedResult = await _adoUow.DriverRepository.GetPaginatedDriversAsync(adoParams);
            var dtos = _mapper.Map<List<ShortDriverResponceDTO>>(pagedResult);

            return new PagedResponseDTO<ShortDriverResponceDTO>
            {
                Items = dtos,
                CurrentPage = pagedResult.CurrentPage,
                TotalPages = pagedResult.TotalPages,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount,
                HasPrevious = pagedResult.HasPrevious,
                HasNext = pagedResult.HasNext
            };
        }
        else
        {
            var pagedResult = await _efUow.EFDriverRepository.GetPaginatedDriversAsync(driverParameters, cancellationToken);
            var dtos = _mapper.Map<List<ShortDriverResponceDTO>>(pagedResult);

            return new PagedResponseDTO<ShortDriverResponceDTO>
            {
                Items = dtos,
                CurrentPage = pagedResult.CurrentPage,
                TotalPages = pagedResult.TotalPages,
                PageSize = pagedResult.PageSize,
                TotalCount = pagedResult.TotalCount,
                HasPrevious = pagedResult.HasPrevious,
                HasNext = pagedResult.HasNext
            };
        }
    }

    /// <summary>
    /// Отримання агрегованого фінансового звіту витрат водія
    /// </summary>
    public async Task<DriverExpenseSummaryResponseDTO> GetDriverExpenseSummaryAsync(
        int driverId,
        DateTime? fromDate,
        DateTime? toDate,
        DataAccessProvider provider,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Запит фінансового звіту водія {DriverId} через {Provider}", driverId, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var summary = await _adoUow.DriverRepository.GetDriverExpenseSummaryAsync(driverId, fromDate, toDate);
            return _mapper.Map<DriverExpenseSummaryResponseDTO>(summary);
        }
        else
        {
            var summary = await _efUow.EFDriverRepository.GetDriverExpenseSummaryAsync(driverId, fromDate, toDate, cancellationToken);
            return _mapper.Map<DriverExpenseSummaryResponseDTO>(summary);
        }
    }

    /// <summary>
    /// Топ-N водіїв за загальними витратами компанії
    /// </summary>
    public async Task<IEnumerable<TopDriverExpenseResponseDTO>> GetTopDriversByExpensesAsync(
        int topCount,
        DataAccessProvider provider,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Запит Топ-{TopCount} водіїв за витратами через {Provider}", topCount, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var topDrivers = await _adoUow.DriverRepository.GetTopDriversByExpensesAsync(topCount);
            return _mapper.Map<IEnumerable<TopDriverExpenseResponseDTO>>(topDrivers);
        }
        else
        {
            var topDrivers = await _efUow.EFDriverRepository.GetTopDriversByExpensesAsync(topCount, cancellationToken);
            return _mapper.Map<IEnumerable<TopDriverExpenseResponseDTO>>(topDrivers);
        }
    }

    /// <summary>
    /// Водії, термін дії посвідчення яких спливає протягом заданої кількості днів
    /// </summary>
    public async Task<IEnumerable<DriverLicenseAlertResponseDTO>> GetDriversWithExpiringLicenseAsync(
        int daysThreshold,
        DataAccessProvider provider,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Пошук водіїв із терміном посвідчення <= {Days} днів через {Provider}", daysThreshold, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var alerts = await _adoUow.DriverRepository.GetDriversWithExpiringLicenseAsync(daysThreshold);
            return _mapper.Map<IEnumerable<DriverLicenseAlertResponseDTO>>(alerts);
        }
        else
        {
            var alerts = await _efUow.EFDriverRepository.GetDriversWithExpiringLicenseAsync(daysThreshold, cancellationToken);
            return _mapper.Map<IEnumerable<DriverLicenseAlertResponseDTO>>(alerts);
        }
    }

    /// <summary>
    /// Пряме порівняння швидкодії запиту повної сутності водія між EF Core та Dapper
    /// </summary>
    public async Task<QueryComparisonResultDTO<FullDriverResponceDTO>> CompareDriverQueryPerformanceAsync(
        int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Запуск порівняння швидкодії GetCompleteEntityAsync для водія {Id}", id);

        // 1. Замір EF Core
        var swEf = Stopwatch.StartNew();
        var efDriver = await _efUow.EFDriverRepository.GetCompleteEntityAsync(id, cancellationToken);
        swEf.Stop();
        var efTime = swEf.Elapsed.TotalMilliseconds;

        // 2. Замір Dapper
        var swDapper = Stopwatch.StartNew();
        var dapperDriver = await _adoUow.DriverRepository.GetCompleteEntityAsync(id);
        swDapper.Stop();
        var dapperTime = swDapper.Elapsed.TotalMilliseconds;

        var mappedResult = efDriver != null
            ? _mapper.Map<FullDriverResponceDTO>(efDriver)
            : (dapperDriver != null ? _mapper.Map<FullDriverResponceDTO>(dapperDriver) : null);

        var faster = dapperTime <= efTime ? "Dapper" : "Entity Framework Core";
        var speedup = efTime > 0 && dapperTime > 0
            ? Math.Round(Math.Max(efTime, dapperTime) / Math.Min(efTime, dapperTime), 2)
            : 1.0;

        var isConsistent = (efDriver == null && dapperDriver == null) ||
                           (efDriver != null && dapperDriver != null &&
                            efDriver.Id == dapperDriver.Id &&
                            efDriver.FirstName == dapperDriver.FirstName &&
                            efDriver.LastName == dapperDriver.LastName &&
                            (efDriver.Expenses?.Count ?? 0) == (dapperDriver.Expenses?.Count ?? 0));

        return new QueryComparisonResultDTO<FullDriverResponceDTO>
        {
            QueryName = "GetCompleteDriverEntity",
            Result = mappedResult,
            EntityFrameworkExecutionTimeMs = Math.Round(efTime, 3),
            DapperExecutionTimeMs = Math.Round(dapperTime, 3),
            FasterProvider = faster,
            SpeedupFactor = speedup,
            DataConsistent = isConsistent,
            Notes = isConsistent
                ? $"Дані з обох провайдерів повністю співпадають. Кількість витрат: {mappedResult?.Expenses?.Count ?? 0}."
                : "Виявлено невідповідність між даними, отриманими з EF Core та Dapper."
        };
    }

    /// <summary>
    /// Пряме порівняння швидкодії агрегатного фінансового звіту між EF Core та Dapper
    /// </summary>
    public async Task<QueryComparisonResultDTO<DriverExpenseSummaryResponseDTO>> CompareExpenseSummaryPerformanceAsync(
        int driverId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Запуск порівняння швидкодії GetDriverExpenseSummaryAsync для водія {DriverId}", driverId);

        // 1. Замір EF Core
        var swEf = Stopwatch.StartNew();
        var efSummary = await _efUow.EFDriverRepository.GetDriverExpenseSummaryAsync(driverId, fromDate, toDate, cancellationToken);
        swEf.Stop();
        var efTime = swEf.Elapsed.TotalMilliseconds;

        // 2. Замір Dapper
        var swDapper = Stopwatch.StartNew();
        var dapperSummary = await _adoUow.DriverRepository.GetDriverExpenseSummaryAsync(driverId, fromDate, toDate);
        swDapper.Stop();
        var dapperTime = swDapper.Elapsed.TotalMilliseconds;

        var mappedResult = _mapper.Map<DriverExpenseSummaryResponseDTO>(efSummary);

        var faster = dapperTime <= efTime ? "Dapper" : "Entity Framework Core";
        var speedup = efTime > 0 && dapperTime > 0
            ? Math.Round(Math.Max(efTime, dapperTime) / Math.Min(efTime, dapperTime), 2)
            : 1.0;

        var isConsistent = efSummary.ExpenseCount == dapperSummary.ExpenseCount &&
                           efSummary.TotalDriverPayment == dapperSummary.TotalDriverPayment &&
                           efSummary.TotalFuelCost == dapperSummary.TotalFuelCost &&
                           efSummary.TotalMaintenanceCost == dapperSummary.TotalMaintenanceCost &&
                           efSummary.GrandTotal == dapperSummary.GrandTotal;

        return new QueryComparisonResultDTO<DriverExpenseSummaryResponseDTO>
        {
            QueryName = "GetDriverExpenseSummary",
            Result = mappedResult,
            EntityFrameworkExecutionTimeMs = Math.Round(efTime, 3),
            DapperExecutionTimeMs = Math.Round(dapperTime, 3),
            FasterProvider = faster,
            SpeedupFactor = speedup,
            DataConsistent = isConsistent,
            Notes = isConsistent
                ? $"Агрегація ідентична: Загальні витрати = {efSummary.GrandTotal:C}, кількість операцій = {efSummary.ExpenseCount}."
                : "Виявлено розбіжність у розрахунках між EF Core та Dapper."
        };
    }

    /// <summary>
    /// Додавання нового водія через обраний провайдер
    /// </summary>
    public async Task<FullDriverResponceDTO> AddDriverToSystemAsync(
        MiniDriverReqDTO driverDTO,
        DataAccessProvider provider,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Додавання водія через {Provider}", provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var entity = _mapper.Map<ADODriver>(driverDTO);
            entity.CreatedAt = DateTime.UtcNow;
            var newId = await _adoUow.DriverRepository.AddAsync(entity);
            var created = await _adoUow.DriverRepository.GetCompleteEntityAsync(newId);
            return _mapper.Map<FullDriverResponceDTO>(created);
        }
        else
        {
            try
            {
                await _efUow.BeginTransactionAsync(cancellationToken);
                var entity = _mapper.Map<EFDriver>(driverDTO);
                entity.CreatedAt = DateTime.UtcNow;
                var added = await _efUow.EFDriverRepository.AddAsync(entity, cancellationToken);
                await _efUow.CompleteAsync(cancellationToken);
                await _efUow.CommitTransactionAsync(cancellationToken);

                var created = await _efUow.EFDriverRepository.GetCompleteEntityAsync(added.Id, cancellationToken);
                return _mapper.Map<FullDriverResponceDTO>(created);
            }
            catch (OperationCanceledException)
            {
                await _efUow.RollbackTransactionAsync(CancellationToken.None);
                throw;
            }
            catch (Exception ex)
            {
                await _efUow.RollbackTransactionAsync(CancellationToken.None);
                _logger.LogError(ex, "Помилка додавання водія через EF Core");
                throw new ApplicationException("Error in transaction while adding driver", ex);
            }
        }
    }

    /// <summary>
    /// Оновлення існуючого водія через обраний провайдер
    /// </summary>
    public async Task<FullDriverResponceDTO> UpdateDriverInSystemAsync(
        int id,
        MiniDriverReqDTO driverDTO,
        DataAccessProvider provider,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Оновлення водія {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var existing = await _adoUow.DriverRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new NotFoundException($"Водія з Id {id} не знайдено для оновлення.");
            }

            existing.FirstName = driverDTO.FirstName;
            existing.LastName = driverDTO.LastName;
            existing.MiddleName = driverDTO.MiddleName;
            existing.Gender = driverDTO.Gender;
            existing.Birthdate = driverDTO.Birthdate;
            existing.DriverLicenseNumber = driverDTO.DriverLicenseNumber;
            existing.DriverLicenseExpirationDate = driverDTO.DriverLicenseExpirationDate ?? existing.DriverLicenseExpirationDate;
            existing.UpdatedAt = DateTime.UtcNow;

            await _adoUow.DriverRepository.ReplaceAsync(existing);
            var updated = await _adoUow.DriverRepository.GetCompleteEntityAsync(id);
            return _mapper.Map<FullDriverResponceDTO>(updated);
        }
        else
        {
            var existing = await _efUow.EFDriverRepository.GetByIdAsNoTrackingAsync(id, cancellationToken);
            if (existing == null)
            {
                throw new NotFoundException($"Водія з Id {id} не знайдено для оновлення.");
            }

            try
            {
                await _efUow.BeginTransactionAsync(cancellationToken);
                var entityToUpdate = _mapper.Map<EFDriver>(driverDTO);
                entityToUpdate.Id = id;
                entityToUpdate.UpdatedAt = DateTime.UtcNow;

                await _efUow.EFDriverRepository.UpdateAsync(entityToUpdate);
                await _efUow.CompleteAsync(cancellationToken);
                await _efUow.CommitTransactionAsync(cancellationToken);

                var updated = await _efUow.EFDriverRepository.GetCompleteEntityAsync(id, cancellationToken);
                return _mapper.Map<FullDriverResponceDTO>(updated);
            }
            catch (OperationCanceledException)
            {
                await _efUow.RollbackTransactionAsync(CancellationToken.None);
                throw;
            }
            catch (Exception ex)
            {
                await _efUow.RollbackTransactionAsync(CancellationToken.None);
                _logger.LogError(ex, "Помилка оновлення водія через EF Core");
                throw new ApplicationException("Error in transaction while updating driver", ex);
            }
        }
    }

    /// <summary>
    /// Видалення водія через обраний провайдер
    /// </summary>
    public async Task DeleteDriverFromSystemAsync(
        int id,
        DataAccessProvider provider,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Видалення водія {Id} через {Provider}", id, provider);

        if (provider == DataAccessProvider.Dapper)
        {
            var existing = await _adoUow.DriverRepository.GetByIdAsync(id);
            if (existing == null)
            {
                throw new NotFoundException($"Водія з Id {id} не знайдено для видалення.");
            }

            await _adoUow.DriverRepository.DeleteAsync(id);
        }
        else
        {
            var existing = await _efUow.EFDriverRepository.GetByIdAsNoTrackingAsync(id, cancellationToken);
            if (existing == null)
            {
                throw new NotFoundException($"Водія з Id {id} не знайдено для видалення.");
            }

            try
            {
                await _efUow.BeginTransactionAsync(cancellationToken);
                await _efUow.EFDriverRepository.DeleteByIdAsync(id, cancellationToken);
                await _efUow.CompleteAsync(cancellationToken);
                await _efUow.CommitTransactionAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await _efUow.RollbackTransactionAsync(CancellationToken.None);
                throw;
            }
            catch (Exception ex)
            {
                await _efUow.RollbackTransactionAsync(CancellationToken.None);
                _logger.LogError(ex, "Помилка видалення водія через EF Core");
                throw new ApplicationException("Error in transaction while deleting driver", ex);
            }
        }
    }

    // =========================================================================================================
    // ПРИКЛАД 1 ТОЛСТОЇ БІЗНЕС-ЛОГІКИ: КОМПЛЕКСНА ОЦІНКА ДОПУСКУ ВОДІЯ ДО РЕЙСУ ТА РОЗРАХУНОК КОШТОРИСУ
    // =========================================================================================================
    /// <summary>
    /// ДЕМОНСТРАЦІЯ КОНЦЕПЦІЇ БІЗНЕС-ЛОГІКИ (BUSINESS LOGIC LAYER / BLL):
    /// Цей метод ілюструє, чим насправді є бізнес-логіка в корпоративних системах.
    /// 
    /// ЧОМУ ЦЕЙ КОД ЗНАХОДИТЬСЯ САМЕ В BLL, А НЕ В КОНТРОЛЕРІ ЧИ РЕПОЗИТОРІЇ:
    /// 1. Чому НЕ в контролері (Controller)?
    ///    Контролер — це лише точка входу HTTP (транспортний протокол). Його обов'язок — прийняти JSON,
    ///    перевірити базовий формат запиту і повернути код стану (200, 400). Якщо помістити сюди 150 рядків
    ///    правил допуску до рейсу, контролер стане «товстим», нетестованим без HTTP-контексту, і ці правила
    ///    неможливо буде повторно використати (наприклад, у консольній черзі RabbitMQ, десктопному WPF або gRPC).
    /// 
    /// 2. Чому НЕ в репозиторії (DAL)?
    ///    Репозиторій відповідає лише за читання/запис рядків у SQL Server ("SELECT * FROM Drivers").
    ///    Він не знає про комерційні політики компанії, правила безпеки перевезень ADR чи формули преміювання.
    ///    База даних має бути максимально ізольованою від логіки додатку.
    /// 
    /// 3. Що саме робить BLL у цьому методі:
    ///    - Застосовує бізнес-правила безпеки праці (віковий ценз, терміни дії посвідчень, статуси).
    ///    - Інспектує стан техніки (вантажівки) через окремі джерела даних.
    ///    - Розраховує комерційний кошторис (тариф, надбавки за стаж, доплату за небезпечність, добові).
    ///    - Приймає автоматичне бізнес-рішення (Approved / ConditionalApproval / Rejected).
    ///    - У разі успіху оркеструє збереження транзакції нового запису витрат через обраний провайдер.
    /// </summary>
    public async Task<DriverTripDispatchAssessmentDTO> AssessAndDispatchDriverForTripAsync(
        DriverTripDispatchRequestDTO request,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Початок комплексної бізнес-оцінки допуску водія {DriverId} на авто {TruckId} (провайдер {Provider})",
            request.DriverId, request.TruckId, provider);

        var result = new DriverTripDispatchAssessmentDTO
        {
            DriverId = request.DriverId,
            TruckId = request.TruckId
        };

        // -----------------------------------------------------------------------------------------------------
        // ЕТАП 1 БІЗНЕС-ЛОГІКИ: ЗБІР ДАНИХ З РІЗНИХ ПІДСИСТЕМ (ОРКЕСТРАЦІЯ РЕПОЗИТОРІЇВ)
        // -----------------------------------------------------------------------------------------------------
        // Бізнес-логіка не зберігає дані сама — вона запитує потрібні агрегати через репозиторії
        string driverFullName;
        string? employmentStatus;
        DateTime birthdate;
        DateTime licenseExpDate;
        string? licenseCategory;
        DateTime? employmentStart;
        string truckInfo;
        bool hasRecentFailedInspection = false;
        DateTime? lastInspectionDate = null;

        if (provider == DataAccessProvider.Dapper)
        {
            var driver = await _adoUow.DriverRepository.GetByIdAsync(request.DriverId);
            if (driver == null)
            {
                throw new NotFoundException($"Водія з Id {request.DriverId} не знайдено в системі (Dapper).");
            }

            driverFullName = $"{driver.LastName} {driver.FirstName} {driver.MiddleName}".Trim();
            employmentStatus = driver.EmploymentStatus;
            birthdate = driver.Birthdate;
            licenseExpDate = driver.DriverLicenseExpirationDate;
            licenseCategory = driver.DriverLicenseCategory;
            employmentStart = driver.EmploymentStartDate;

            var truck = await _adoUow.TruckRepository.GetAsync(request.TruckId);
            truckInfo = $"{truck.Model} (Держномер: {truck.TruckNumber})";

            var inspections = (await _adoUow.InspectionRepository.GetInspectionsByTruckIdAsync(request.TruckId)).ToList();
            if (inspections.Any())
            {
                var latest = inspections.OrderByDescending(i => i.InspectionDate).First();
                lastInspectionDate = latest.InspectionDate;
                hasRecentFailedInspection = !latest.Result;
            }
        }
        else
        {
            var driver = await _efUow.EFDriverRepository.GetByIdAsync(request.DriverId, cancellationToken);
            if (driver == null)
            {
                throw new NotFoundException($"Водія з Id {request.DriverId} не знайдено в системі (EF Core).");
            }

            driverFullName = $"{driver.LastName} {driver.FirstName} {driver.MiddleName}".Trim();
            employmentStatus = driver.EmploymentStatus;
            birthdate = driver.Birthdate;
            licenseExpDate = driver.DriverLicenseExpirationDate;
            licenseCategory = driver.DriverLicenseCategory;
            employmentStart = driver.EmploymentStartDate;

            var truck = await _efUow.EFPTruckRepository.GetByIdAsync(request.TruckId);
            truckInfo = $"{truck.Model} (Держномер: {truck.TruckNumber})";

            var inspections = (await _efUow.EFInspectionRepository.GetInspectionsByTruckIdAsync(request.TruckId)).ToList();
            if (inspections.Any())
            {
                var latest = inspections.OrderByDescending(i => i.InspectionDate).First();
                lastInspectionDate = latest.InspectionDate;
                hasRecentFailedInspection = !latest.Result;
            }
        }

        result.DriverFullName = driverFullName;
        result.TruckInfo = truckInfo;

        // -----------------------------------------------------------------------------------------------------
        // ЕТАП 2 БІЗНЕС-ЛОГІКИ: ПЕРЕВІРКА ЮРИДИЧНИХ ТА КОРПОРАТИВНИХ ПРАВИЛ (COMPLIANCE ENFORCEMENT)
        // -----------------------------------------------------------------------------------------------------
        // Правило 1: Статус працевлаштування. Лише активний співробітник може бути скерований у рейс.
        if (!string.Equals(employmentStatus, "Active", StringComparison.OrdinalIgnoreCase))
        {
            result.CriticalViolations.Add($"Водій має статус працевлаштування '{employmentStatus ?? "Не визначено"}'. Виїзд у рейс заборонено корпоративною політикою.");
        }

        // Правило 2: Віковий ценз водія.
        // Корпоративне правило: водій вантажівки має бути не молодшим 21 року,
        // а для перевезення небезпечних вантажів (ADR) — мінімум 25 років з досвідом.
        var today = DateTime.UtcNow.Date;
        var age = today.Year - birthdate.Year;
        if (birthdate.Date > today.AddYears(-age)) age--;

        bool isDangerousCargo = request.CargoType.Contains("Небезпечний", StringComparison.OrdinalIgnoreCase) ||
                                request.CargoType.Contains("ADR", StringComparison.OrdinalIgnoreCase);

        if (age < 21)
        {
            result.CriticalViolations.Add($"Вік водія ({age} р.) менший за обов'язковий мінімум для далекобійних рейсів (21 р.).");
        }
        else if (isDangerousCargo && age < 25)
        {
            result.CriticalViolations.Add($"Для категорії 'Небезпечний вантаж (ADR)' вік водія має бути від 25 років (поточний: {age} р.).");
        }

        // Правило 3: Категорія посвідчення водія. Для вантажних перевезень обов'язкові категорії C або CE.
        var cat = licenseCategory ?? string.Empty;
        if (!cat.Contains('C', StringComparison.OrdinalIgnoreCase))
        {
            result.CriticalViolations.Add($"У водія відсутня вантажна категорія водійських прав 'C' або 'CE' (наявні категорії: '{cat}').");
        }

        // Правило 4: Термін дії водійського посвідчення.
        // Бізнес-правило: посвідчення водія має бути дійсним увесь рейс + додатковий буфер безпеки (мінімум 14 днів).
        var tripEndDate = today.AddDays(request.EstimatedDurationDays);
        var requiredValidityDate = tripEndDate.AddDays(14);
        if (licenseExpDate < requiredValidityDate)
        {
            if (licenseExpDate < tripEndDate)
            {
                result.CriticalViolations.Add($"Термін дії посвідчення ({licenseExpDate:dd.MM.yyyy}) спливає ПІД ЧАС рейсу (завершення рейсу: {tripEndDate:dd.MM.yyyy}). Виїзд категорично заборонено.");
            }
            else
            {
                result.WarningsAndRecommendations.Add($"Термін дії посвідчення ({licenseExpDate:dd.MM.yyyy}) спливає менш ніж через 14 днів після повернення. Рекомендовано оновити документи.");
            }
        }

        // -----------------------------------------------------------------------------------------------------
        // ЕТАП 3 БІЗНЕС-ЛОГІКИ: ПЕРЕВІРКА ТЕХНІЧНОГО СТАНУ АВТОТРАНСПОРТУ (FLEET SAFETY POLICY)
        // -----------------------------------------------------------------------------------------------------
        // Правило 5: Стан останнього техогляду. Якщо авто провалило останній техогляд — виїзд заблоковано.
        if (hasRecentFailedInspection)
        {
            result.CriticalViolations.Add($"Вантажівка {truckInfo} має статус останнього техогляду 'НЕ ПРОЙДЕНО'. Експлуатація аварійно небезпечна.");
        }

        // Правило 6: Періодичність техоглядів. Техогляд не повинен бути давнішим за 180 календарних днів.
        if (lastInspectionDate.HasValue)
        {
            var daysSinceInspection = (today - lastInspectionDate.Value.Date).TotalDays;
            if (daysSinceInspection > 180)
            {
                result.WarningsAndRecommendations.Add($"Останній техогляд проведено {daysSinceInspection:F0} днів тому (норма компанії — раз на 180 днів). Потрібен позачерговий огляд механіком.");
            }
        }
        else
        {
            result.WarningsAndRecommendations.Add("У базі даних відсутні записи про проходження техоглядів для даного авто.");
        }

        // -----------------------------------------------------------------------------------------------------
        // ЕТАП 4 БІЗНЕС-ЛОГІКИ: КОМЕРЦІЙНИЙ РОЗРАХУНОК КОШТОРИСУ ТА ВИНАГОРОДИ (PRICING ENGINE)
        // -----------------------------------------------------------------------------------------------------
        // Розрахунок 1: Базова заробітна плата водія = дистанція * базовий тариф
        result.BaseDriverPayment = Math.Round(request.DistanceKm * request.BaseRatePerKm, 2);

        // Розрахунок 2: Надбавка за безаварійний стаж у компанії
        var yearsOfService = employmentStart.HasValue ? (today.Year - employmentStart.Value.Year) : 0;
        decimal experienceMultiplier = 0.0m;
        if (yearsOfService >= 5)
        {
            experienceMultiplier = 0.20m; // +20% до ставки за стаж від 5 років
        }
        else if (yearsOfService >= 2)
        {
            experienceMultiplier = 0.10m; // +10% до ставки за стаж від 2 років
        }
        result.ExperienceBonus = Math.Round(result.BaseDriverPayment * experienceMultiplier, 2);

        // Розрахунок 3: Доплата за складність/ризик вантажу (ADR, міжнародний, рефрижератор)
        decimal riskMultiplier = 0.0m;
        if (isDangerousCargo)
        {
            riskMultiplier = 0.35m; // +35% надбавка за небезпечний вантаж
        }
        else if (request.CargoType.Contains("Міжнародний", StringComparison.OrdinalIgnoreCase))
        {
            riskMultiplier = 0.20m; // +20% надбавка за перетин кордону та митні процедури
        }
        else if (request.CargoType.Contains("Швидкопсувний", StringComparison.OrdinalIgnoreCase))
        {
            riskMultiplier = 0.15m; // +15% надбавка за температурний контроль рефрижератора
        }
        result.CargoRiskBonus = Math.Round(result.BaseDriverPayment * riskMultiplier, 2);

        // Розрахунок 4: Добові витрати водія (Per Diem) = 800 грн на добу
        const decimal standardDailyPerDiem = 800.0m;
        result.PerDiemAllowance = request.EstimatedDurationDays * standardDailyPerDiem;

        // Підсумкова виплата водієві
        result.TotalDriverCompensation = result.BaseDriverPayment + result.ExperienceBonus + result.CargoRiskBonus + result.PerDiemAllowance;

        // Розрахунок 5: Прогноз вартості пального
        // Середня норма витрат важкої вантажівки = 32 літри на 100 км, середня вартість пального ДП = 54.50 грн/л
        const decimal fuelConsumptionPer100Km = 32.0m;
        const decimal averageDieselPricePerLiter = 54.50m;
        var estimatedFuelLiters = (request.DistanceKm / 100.0m) * fuelConsumptionPer100Km;
        result.EstimatedFuelExpense = Math.Round(estimatedFuelLiters * averageDieselPricePerLiter, 2);

        // Розрахунок 6: Резервний фонд на непередбачені дорожні витрати (10% від пального + обслуговування)
        result.EmergencyReserveFund = Math.Round(result.EstimatedFuelExpense * 0.10m, 2);

        // Повний плановий бюджет рейсу
        result.TotalTripBudget = result.TotalDriverCompensation + result.EstimatedFuelExpense + result.EmergencyReserveFund;

        // -----------------------------------------------------------------------------------------------------
        // ЕТАП 5 БІЗНЕС-ЛОГІКИ: ПРИЙНЯТТЯ РІШЕННЯ ТА ТРАНЗАКЦІЙНИЙ ВИСНОВОК (DECISION & WORKFLOW)
        // -----------------------------------------------------------------------------------------------------
        if (result.CriticalViolations.Count > 0)
        {
            // Бізнес-вердикт: Відхилено
            result.DecisionStatus = "Rejected";
            result.IsDispatchAllowed = false;
            result.DecisionSummary = $"Виїзд у рейс ЗАБОРОНЕНО. Виявлено {result.CriticalViolations.Count} критичних порушень корпоративних правил безпеки.";
        }
        else if (result.WarningsAndRecommendations.Count > 0)
        {
            // Бізнес-вердикт: Умовне схвалення (вимагає додаткового погодження старшого диспетчера)
            result.DecisionStatus = "ConditionalApproval";
            result.IsDispatchAllowed = true;
            result.DecisionSummary = "Рейс попередньо ПОГОДЖЕНО з застереженнями. Вимагає візи керівника колони перед видачею подорожнього листа.";
        }
        else
        {
            // Бізнес-вердикт: Повністю схвалено
            result.DecisionStatus = "Approved";
            result.IsDispatchAllowed = true;
            result.DecisionSummary = "Рейс повністю СХВАЛЕНО. Усі юридичні, технічні та фінансові норми компанії дотримано.";
        }

        // -----------------------------------------------------------------------------------------------------
        // ЕТАП 6 БІЗНЕС-ЛОГІКИ: ОРКЕСТРАЦІЯ ЗБЕРЕЖЕННЯ ПЛАНОВИХ ВИТРАТ ПРИ УСПІШНОМУ СХВАЛЕННІ
        // -----------------------------------------------------------------------------------------------------
        // Якщо виїзд дозволено — бізнес-логіка автоматично резервує фінансовий бюджет у системі
        if (result.IsDispatchAllowed)
        {
            var note = $"[План рейсу: {request.CargoType}, {request.DistanceKm} км, статус: {result.DecisionStatus}]";

            if (provider == DataAccessProvider.Dapper)
            {
                var expenseEntity = new Drivers.DAL.ADO.Entities.Expenses
                {
                    DriverID = request.DriverId,
                    TruckID = request.TruckId,
                    DriverPayment = result.TotalDriverCompensation,
                    FuelCost = result.EstimatedFuelExpense,
                    MaintenanceCost = result.EmergencyReserveFund,
                    Category = "Запланований рейс",
                    Date = today,
                    Note = note,
                    CreatedAt = DateTime.UtcNow
                };

                var newExpenseId = await _adoUow.ExpensesRepository.AddAsync(expenseEntity);
                result.CreatedExpensePlanId = newExpenseId;
            }
            else
            {
                var efExpense = new EFExpense
                {
                    DriverId = request.DriverId,
                    TruckId = request.TruckId,
                    DriverPayment = result.TotalDriverCompensation,
                    FuelCost = result.EstimatedFuelExpense,
                    MaintenanceCost = result.EmergencyReserveFund,
                    Category = "Запланований рейс",
                    Date = today,
                    Note = note,
                    CreatedAt = DateTime.UtcNow
                };

                var added = await _efUow.EFExpenseRepository.AddAsync(efExpense);
                await _efUow.CompleteAsync(cancellationToken);
                result.CreatedExpensePlanId = added.Id;
            }

            _logger.LogInformation("Успішно сформовано плановий бюджет рейсу {ExpenseId} на суму {Budget:C}",
                result.CreatedExpensePlanId, result.TotalTripBudget);
        }

        return result;
    }

    // =========================================================================================================
    // ПРИКЛАД 2 ТОЛСТОЇ БІЗНЕС-ЛОГІКИ: РІЧНИЙ АУДИТ ЕФЕКТИВНОСТІ ВОДІЯ ТА РОЗРАХУНОК КОРПОРАТИВНИХ БОНУСІВ
    // =========================================================================================================
    /// <summary>
    /// ДЕМОНСТРАЦІЯ КОНЦЕПЦІЇ БІЗНЕС-ЛОГІКИ:
    /// Цей метод ілюструє аналітичну та оціночну складову бізнес-логіки.
    /// 
    /// СУТЬ БІЗНЕС-ЗАДАЧІ:
    /// Компанія не просто показує сухі цифри з БД, а інтерпретує їх за внутрішньою бізнес-моделлю:
    /// 1. Агрегує фінансові та експлуатаційні транзакції водія за звітний рік.
    /// 2. Розраховує 3 комплексні індекси KPI (економія пального, дбайливість до авто, дисципліна).
    /// 3. Обчислює інтегральний рейтинг водія (Composite Score) за ваговими коефіцієнтами компанії.
    /// 4. Присвоює кваліфікаційний корпоративний грейд (Grade A/B/C або переатестація).
    /// 5. Обчислює персональну річну премію та автоматично реєструє нарахування бонусу в базі.
    /// </summary>
    public async Task<DriverAnnualAuditReportDTO> ConductDriverAnnualPerformanceAuditAsync(
        int driverId,
        int auditYear,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Запуск щорічного аудиту ефективності водія {DriverId} за {Year} рік через {Provider}",
            driverId, auditYear, provider);

        var report = new DriverAnnualAuditReportDTO
        {
            DriverId = driverId,
            AuditYear = auditYear
        };

        // -----------------------------------------------------------------------------------------------------
        // КРОК 1: ЗБІР ІСТОРИЧНИХ ДАНИХ З РІЗНИХ ТАБЛИЦЬ (DAL)
        // -----------------------------------------------------------------------------------------------------
        string driverFullName;
        DateTime? licenseExpDate;
        List<(decimal DriverPayment, decimal FuelCost, decimal MaintenanceCost, string Category, DateTime Date, int? TruckId)> expenses;

        if (provider == DataAccessProvider.Dapper)
        {
            var driver = await _adoUow.DriverRepository.GetByIdAsync(driverId);
            if (driver == null)
            {
                throw new NotFoundException($"Водія з Id {driverId} не знайдено для проведення аудиту.");
            }
            driverFullName = $"{driver.LastName} {driver.FirstName}".Trim();
            licenseExpDate = driver.DriverLicenseExpirationDate;

            var allExpenses = await _adoUow.ExpensesRepository.GetExpensesByDriverIdAsync(driverId);
            expenses = allExpenses
                .Where(e => e.Date.Year == auditYear)
                .Select(e => (e.DriverPayment, e.FuelCost, e.MaintenanceCost, e.Category, e.Date, (int?)e.TruckID))
                .ToList();
        }
        else
        {
            var driver = await _efUow.EFDriverRepository.GetByIdAsync(driverId, cancellationToken);
            if (driver == null)
            {
                throw new NotFoundException($"Водія з Id {driverId} не знайдено для проведення аудиту.");
            }
            driverFullName = $"{driver.LastName} {driver.FirstName}".Trim();
            licenseExpDate = driver.DriverLicenseExpirationDate;

            var allExpenses = await _efUow.EFExpenseRepository.GetExpensesByDriverIdAsync(driverId);
            expenses = allExpenses
                .Where(e => e.Date.Year == auditYear)
                .Select(e => (e.DriverPayment, e.FuelCost, e.MaintenanceCost, e.Category, e.Date, (int?)e.TruckId))
                .ToList();
        }

        report.DriverFullName = driverFullName;
        report.TotalTripsCompleted = expenses.Count;

        // Якщо за вказаний рік водій не виконав жодного рейсу — формується відповідний висновок
        if (expenses.Count == 0)
        {
            report.AssignedDriverGrade = "NoActivity (Немає активності)";
            report.OverallPerformanceRating = 0.0;
            report.ExecutiveRecommendations.Add($"У {auditYear} році водій не виконував комерційних рейсів. Аудит зафіксував відсутність активності.");
            return report;
        }

        // -----------------------------------------------------------------------------------------------------
        // КРОК 2: АГРЕГАЦІЯ ФІНАНСОВИХ ПОКАЗНИКІВ
        // -----------------------------------------------------------------------------------------------------
        report.TotalEarnings = expenses.Sum(e => e.DriverPayment);
        report.TotalFuelCostIncurred = expenses.Sum(e => e.FuelCost);
        report.TotalVehicleMaintenanceIncurred = expenses.Sum(e => e.MaintenanceCost);
        report.GrandTotalOperationalExpenses = report.TotalEarnings + report.TotalFuelCostIncurred + report.TotalVehicleMaintenanceIncurred;

        // -----------------------------------------------------------------------------------------------------
        // КРОК 3: БІЗНЕС-ФОРМУЛИ РОЗРАХУНКУ ІНДИВІДУАЛЬНИХ KPI (ОЦІНЮВАЛЬНІ АЛГОРИТМИ)
        // -----------------------------------------------------------------------------------------------------
        
        // 1. Індекс паливної економічності (Fuel Efficiency Score, шкала 0 - 100):
        // Бізнес-норматив: пальне в ідеалі складає приблизно 40-45% від операційних витрат на маршруті.
        // Якщо відсоток пального перевищує 55% — це свідчить про агресивний стиль їзди або простої з увімкненим двигуном.
        double fuelScore;
        var fuelRatio = report.GrandTotalOperationalExpenses > 0
            ? (double)(report.TotalFuelCostIncurred / report.GrandTotalOperationalExpenses)
            : 0.5;

        if (fuelRatio <= 0.40)
        {
            fuelScore = 95.0; // Відмінна економія пального
        }
        else if (fuelRatio <= 0.48)
        {
            fuelScore = 85.0; // Стандартна нормальна витрата
        }
        else if (fuelRatio <= 0.55)
        {
            fuelScore = 65.0; // Підвищена витрата пального
        }
        else
        {
            fuelScore = 40.0; // Суттєвий перевитрата
        }
        report.FuelEfficiencyScore = fuelScore;

        // 2. Індекс дбайливого ставлення до автопарку (Vehicle Care Score, шкала 0 - 100):
        // Бізнес-правило: якщо витрати на ремонти перевищують 25% від заробітної плати водія — водій недбало ставиться до авто.
        double vehicleCareScore;
        var maintenanceRatio = report.TotalEarnings > 0
            ? (double)(report.TotalVehicleMaintenanceIncurred / report.TotalEarnings)
            : 0.5;

        if (maintenanceRatio <= 0.10)
        {
            vehicleCareScore = 98.0; // Техніка в ідеальному стані, мінімум ремонтів
        }
        else if (maintenanceRatio <= 0.20)
        {
            vehicleCareScore = 85.0; // Плановий рівень експлуатації
        }
        else if (maintenanceRatio <= 0.35)
        {
            vehicleCareScore = 60.0; // Часті поломки
        }
        else
        {
            vehicleCareScore = 35.0; // Критично висока вартість ремонтів
        }
        report.VehicleCareScore = vehicleCareScore;

        // 3. Індекс дисципліни та безпеки (Safety & Compliance Score, шкала 0 - 100):
        // Враховує дійсність документів та регулярність виконання рейсів
        double safetyScore = 90.0;
        if (licenseExpDate.HasValue && licenseExpDate.Value < DateTime.UtcNow)
        {
            safetyScore -= 40.0; // Штраф за прострочені документи
        }
        if (report.TotalTripsCompleted < 3)
        {
            safetyScore -= 20.0; // Недостатній напрацьований обсяг
        }
        report.SafetyComplianceScore = Math.Max(safetyScore, 10.0);

        // 4. Інтегральний комбінований бал за корпоративною ваговою моделлю:
        // Паливна ефективність (35%) + Дбайливість до техніки (35%) + Дисципліна/Безпека (30%)
        report.OverallPerformanceRating = Math.Round(
            (report.FuelEfficiencyScore * 0.35) +
            (report.VehicleCareScore * 0.35) +
            (report.SafetyComplianceScore * 0.30), 1);

        // -----------------------------------------------------------------------------------------------------
        // КРОК 4: МАТРИЦЯ ПРИСВОЄННЯ КВАЛІФІКАЦІЙНОГО ГРЕЙДУ ТА ПРЕМІЮВАННЯ (BUSINESS RULES MATRIX)
        // -----------------------------------------------------------------------------------------------------
        if (report.OverallPerformanceRating >= 85.0)
        {
            // Платиновий грейд: зразковий водій, найвищий рівень довіри
            report.AssignedDriverGrade = "Grade A (Platinum)";
            // Премія: 10% від річного заробітку
            report.CalculatedAnnualBonus = Math.Round(report.TotalEarnings * 0.10m, 2);

            report.ExecutiveRecommendations.Add("Рекомендовано для преміальних міжнародних перевезень та перевезення особливо цінних вантажів.");
            report.ExecutiveRecommendations.Add($"Призначено максимальний річний бонус у розмірі {report.CalculatedAnnualBonus:C}.");
            report.ExecutiveRecommendations.Add("Рекомендовано кандидатом на роль водія-наставника для молодих спеціалістів.");
        }
        else if (report.OverallPerformanceRating >= 70.0)
        {
            // Золотий грейд: надійний кваліфікований спеціаліст
            report.AssignedDriverGrade = "Grade B (Gold)";
            // Премія: 5% від річного заробітку
            report.CalculatedAnnualBonus = Math.Round(report.TotalEarnings * 0.05m, 2);

            report.ExecutiveRecommendations.Add("Стабільний та надійний співробітник. Допускається до всіх типів внутрішніх та міжнародних маршрутів.");
            report.ExecutiveRecommendations.Add($"Призначено річну премію за надійність у розмірі {report.CalculatedAnnualBonus:C}.");
        }
        else if (report.OverallPerformanceRating >= 50.0)
        {
            // Срібний грейд: задовільний рівень з зауваженнями
            report.AssignedDriverGrade = "Grade C (Silver)";
            report.CalculatedAnnualBonus = 0.0m;

            report.ExecutiveRecommendations.Add("Середній рівень ефективності. Виявлено резерв для оптимізації паливних витрат.");
            report.ExecutiveRecommendations.Add("Рекомендовано провести інструктаж з економічного керування вантажівкою (Eco-driving).");
            report.ExecutiveRecommendations.Add("Обмежити участь у складних рейсах з небезпечними вантажами.");
        }
        else
        {
            // Критичний рівень: низький бал, потрібна атестація
            report.AssignedDriverGrade = "UnderReview (Потребує переатестації)";
            report.CalculatedAnnualBonus = 0.0m;

            report.ExecutiveRecommendations.Add("УВАГА: Критично низький інтегральний рейтинг ефективності.");
            report.ExecutiveRecommendations.Add("Призупинити допуск до далеких рейсів до проходження внутрішньої комісії з безпеки праці.");
            report.ExecutiveRecommendations.Add("Направити на обов'язковий повторний технічний огляд авто та перевірку стилю водіння.");
        }

        // -----------------------------------------------------------------------------------------------------
        // КРОК 5: ТРАНЗАКЦІЙНЕ НАРАХУВАННЯ ПРЕМІЇ У ФІНАНСОВОМУ МОДУЛІ (СИСТЕМНА ДІЯ)
        // -----------------------------------------------------------------------------------------------------
        if (report.CalculatedAnnualBonus > 0)
        {
            var bonusNote = $"[Річний аудит {auditYear}: Присвоєно {report.AssignedDriverGrade}, рейтинг {report.OverallPerformanceRating:F1}/100]";
            var firstTruckId = expenses.FirstOrDefault(e => e.TruckId.HasValue).TruckId ?? 1;

            if (provider == DataAccessProvider.Dapper)
            {
                var bonusExpense = new Drivers.DAL.ADO.Entities.Expenses
                {
                    DriverID = driverId,
                    TruckID = firstTruckId,
                    DriverPayment = report.CalculatedAnnualBonus,
                    FuelCost = 0.0m,
                    MaintenanceCost = 0.0m,
                    Category = "Річна премія водія",
                    Date = DateTime.UtcNow.Date,
                    Note = bonusNote,
                    CreatedAt = DateTime.UtcNow
                };
                await _adoUow.ExpensesRepository.AddAsync(bonusExpense);
                report.IsBonusRecordedInExpenses = true;
            }
            else
            {
                var bonusExpense = new EFExpense
                {
                    DriverId = driverId,
                    TruckId = firstTruckId,
                    DriverPayment = report.CalculatedAnnualBonus,
                    FuelCost = 0.0m,
                    MaintenanceCost = 0.0m,
                    Category = "Річна премія водія",
                    Date = DateTime.UtcNow.Date,
                    Note = bonusNote,
                    CreatedAt = DateTime.UtcNow
                };
                await _efUow.EFExpenseRepository.AddAsync(bonusExpense);
                await _efUow.CompleteAsync(cancellationToken);
                report.IsBonusRecordedInExpenses = true;
            }

            _logger.LogInformation("Успішно зафіксовано нарахування річної премії водієві {DriverId} на суму {Bonus:C}",
                driverId, report.CalculatedAnnualBonus);
        }

        return report;
    }
}
