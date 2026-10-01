using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Models;
using Drivers.DAL.EF.Entities.HelpModels;

namespace Drivers.BLL.Managers.Contracts;

/// <summary>
/// Інтерфейс бізнес-менеджера водіїв.
/// Забезпечує повний стек бізнес-логіки з підтримкою двох незалежних провайдерів даних:
/// Entity Framework Core та ADO.NET + Dapper, а також методи прямого бенчмаркінгу.
/// </summary>
public interface IDriversManager
{
    /// <summary>
    /// Отримання списку всіх водіїв
    /// </summary>
    Task<IEnumerable<ShortDriverResponceDTO>> GetListOfAllDrivers(
        DataAccessProvider provider,
        CancellationToken cancellationToken);

    /// <summary>
    /// Отримання повної інформації про водія разом із компанією, фото та витратами
    /// </summary>
    Task<FullDriverResponceDTO> GetFullInfoAboutDriver(
        int id,
        DataAccessProvider provider,
        CancellationToken cancellationToken);

    /// <summary>
    /// Отримання пагінованого списку водіїв із фільтрацією та сортуванням
    /// </summary>
    Task<PagedResponseDTO<ShortDriverResponceDTO>> GetPaginatedDrivers(
        DriverParameters driverParameters,
        DataAccessProvider provider,
        CancellationToken cancellationToken);

    /// <summary>
    /// Отримання зведеного фінансового звіту витрат водія за вказаний період
    /// </summary>
    Task<DriverExpenseSummaryResponseDTO> GetDriverExpenseSummaryAsync(
        int driverId,
        DateTime? fromDate,
        DateTime? toDate,
        DataAccessProvider provider,
        CancellationToken cancellationToken);

    /// <summary>
    /// Топ-N водіїв з найбільшими витратами компанії
    /// </summary>
    Task<IEnumerable<TopDriverExpenseResponseDTO>> GetTopDriversByExpensesAsync(
        int topCount,
        DataAccessProvider provider,
        CancellationToken cancellationToken);

    /// <summary>
    /// Список водіїв, термін дії посвідчення яких закінчується найближчим часом
    /// </summary>
    Task<IEnumerable<DriverLicenseAlertResponseDTO>> GetDriversWithExpiringLicenseAsync(
        int daysThreshold,
        DataAccessProvider provider,
        CancellationToken cancellationToken);

    /// <summary>
    /// Бенчмаркінг: порівняння часу виконання запиту повної сутності водія між EF Core та Dapper
    /// </summary>
    Task<QueryComparisonResultDTO<FullDriverResponceDTO>> CompareDriverQueryPerformanceAsync(
        int id,
        CancellationToken cancellationToken);

    /// <summary>
    /// Бенчмаркінг: порівняння часу виконання агрегаційного фінансового запиту між EF Core та Dapper
    /// </summary>
    Task<QueryComparisonResultDTO<DriverExpenseSummaryResponseDTO>> CompareExpenseSummaryPerformanceAsync(
        int driverId,
        DateTime? fromDate,
        DateTime? toDate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Додавання нового водія до системи
    /// </summary>
    Task<FullDriverResponceDTO> AddDriverToSystemAsync(
        MiniDriverReqDTO driverDTO,
        DataAccessProvider provider,
        CancellationToken cancellationToken);

    /// <summary>
    /// Оновлення існуючого водія в системі
    /// </summary>
    Task<FullDriverResponceDTO> UpdateDriverInSystemAsync(
        int id,
        MiniDriverReqDTO driverDTO,
        DataAccessProvider provider,
        CancellationToken cancellationToken);

    /// <summary>
    /// Видалення водія з системи
    /// </summary>
    Task DeleteDriverFromSystemAsync(
        int id,
        DataAccessProvider provider,
        CancellationToken cancellationToken);

    // =========================================================================
    // ТОЛСТА БІЗНЕС-ЛОГІКА (RICH DOMAIN / APPLICATION BUSINESS LOGIC METHODS)
    // =========================================================================

    /// <summary>
    /// Комплексна бізнес-оцінка готовності водія та допуск до запланованого рейсу.
    /// Перевіряє юридичні вимоги, віковий ценз, терміни посвідчення, безпеку вантажівки,
    /// розраховує повний кошторис (зарплата, добові, пальне, ризики) та формує бізнес-рішення.
    /// </summary>
    Task<DriverTripDispatchAssessmentDTO> AssessAndDispatchDriverForTripAsync(
        DriverTripDispatchRequestDTO request,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Комплексний річний аудит ефективності та надійності водія.
    /// Агрегує фінансові витрати, інциденти, розраховує KPI (паливна ефективність,
    /// дбайливість до автопарку, дисципліна), присвоює корпоративний грейд і розраховує бонусний фонд.
    /// </summary>
    Task<DriverAnnualAuditReportDTO> ConductDriverAnnualPerformanceAuditAsync(
        int driverId,
        int auditYear,
        DataAccessProvider provider,
        CancellationToken cancellationToken = default);
}
