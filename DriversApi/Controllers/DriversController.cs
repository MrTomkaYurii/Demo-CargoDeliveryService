using System.Text.Json;
using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Managers.Contracts;
using Drivers.BLL.Models;
using Drivers.DAL.EF.Entities.HelpModels;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivers.Api.Controllers;

/// <summary>
/// Контролер управління водіями.
/// Підтримує динамічний вибір провайдера даних (Entity Framework Core або Dapper),
/// розширені аналітичні вибірки та прямий бенчмаркінг швидкодії.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DriversController : ControllerBase
{
    private readonly ILogger<DriversController> _logger;
    private readonly IDriversManager _driversManager;

    public DriversController(
        ILogger<DriversController> logger,
        IDriversManager driversManager)
    {
        _logger = logger;
        _driversManager = driversManager;
    }

    /// <summary>
    /// Отримання повної інформації про водія з навігаційними зв'язками (Компанія, Фото, Витрати)
    /// </summary>
    /// <param name="id">Ідентифікатор водія</param>
    /// <param name="provider">Провайдер доступу (0 - EF Core, 1 - Dapper)</param>
    /// <param name="cancellationToken">Токен скасування</param>
    [AllowAnonymous]
    [HttpGet("{id}", Name = "GetInformationAboutDriver")]
    [ProducesResponseType(typeof(FullDriverResponceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FullDriverResponceDTO>> GetFullInfoAboutDriver(
        int id,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var result = await _driversManager.GetFullInfoAboutDriver(id, provider, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Отримання списку всіх водіїв (скорочена модель)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("alldrivers")]
    [ProducesResponseType(typeof(IEnumerable<ShortDriverResponceDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ShortDriverResponceDTO>>> GetAllDriversAsync(
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var result = await _driversManager.GetListOfAllDrivers(provider, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Пагінований список водіїв із фільтрацією за віком та прізвищем
    /// </summary>
    [AllowAnonymous]
    [HttpGet("alldriverspaginated")]
    [ProducesResponseType(typeof(PagedResponseDTO<ShortDriverResponceDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponseDTO<ShortDriverResponceDTO>>> GetAllDriversPaginatedAsync(
        [FromQuery] DriverParameters driverParameters,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (!driverParameters.ValidYearRange)
        {
            return BadRequest("Максимальний рік народження не може бути меншим за мінімальний рік.");
        }

        var result = await _driversManager.GetPaginatedDrivers(driverParameters, provider, cancellationToken);

        var paginationHeader = new
        {
            result.TotalCount,
            result.PageSize,
            result.CurrentPage,
            result.TotalPages,
            result.HasNext,
            result.HasPrevious
        };
        Response.Headers["X-Pagination"] = JsonSerializer.Serialize(paginationHeader);

        return Ok(result);
    }

    /// <summary>
    /// Отримання агрегованої фінансової аналітики витрат водія (паливо, зарплата, обслуговування)
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}/expense-summary")]
    [ProducesResponseType(typeof(DriverExpenseSummaryResponseDTO), StatusCodes.Status200OK)]
    public async Task<ActionResult<DriverExpenseSummaryResponseDTO>> GetDriverExpenseSummary(
        int id,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
        {
            return BadRequest("Початкова дата не може бути пізнішою за кінцеву дату.");
        }

        var result = await _driversManager.GetDriverExpenseSummaryAsync(id, fromDate, toDate, provider, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Отримання Топ-N водіїв з найбільшими загальними витратами для компанії
    /// </summary>
    [AllowAnonymous]
    [HttpGet("top-by-expenses")]
    [ProducesResponseType(typeof(IEnumerable<TopDriverExpenseResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TopDriverExpenseResponseDTO>>> GetTopDriversByExpenses(
        [FromQuery] int top = 5,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (top <= 0) top = 5;
        var result = await _driversManager.GetTopDriversByExpensesAsync(top, provider, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Отримання водіїв, термін дії посвідчення яких закінчується найближчим часом
    /// </summary>
    [AllowAnonymous]
    [HttpGet("expiring-licenses")]
    [ProducesResponseType(typeof(IEnumerable<DriverLicenseAlertResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DriverLicenseAlertResponseDTO>>> GetExpiringLicenses(
        [FromQuery] int daysThreshold = 30,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var result = await _driversManager.GetDriversWithExpiringLicenseAsync(daysThreshold, provider, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Бенчмаркінг: пряме порівняння часу виконання вибірки повної сутності водія між EF Core та Dapper
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}/compare-providers")]
    [ProducesResponseType(typeof(QueryComparisonResultDTO<FullDriverResponceDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<QueryComparisonResultDTO<FullDriverResponceDTO>>> CompareProviders(
        int id,
        CancellationToken cancellationToken = default)
    {
        var comparison = await _driversManager.CompareDriverQueryPerformanceAsync(id, cancellationToken);
        return Ok(comparison);
    }

    /// <summary>
    /// Бенчмаркінг: пряме порівняння часу виконання агрегатного звіту витрат між EF Core та Dapper
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}/compare-expense-summary")]
    [ProducesResponseType(typeof(QueryComparisonResultDTO<DriverExpenseSummaryResponseDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<QueryComparisonResultDTO<DriverExpenseSummaryResponseDTO>>> CompareExpenseSummary(
        int id,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var comparison = await _driversManager.CompareExpenseSummaryPerformanceAsync(id, fromDate, toDate, cancellationToken);
        return Ok(comparison);
    }

    /// <summary>
    /// Додавання нового водія до системи
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PostDriverAsync(
        [FromBody] MiniDriverReqDTO drv,
        [FromServices] IValidator<MiniDriverReqDTO> validator,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await validator.ValidateAsync(drv, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var createdDriver = await _driversManager.AddDriverToSystemAsync(drv, provider, cancellationToken);
        return CreatedAtAction(nameof(GetFullInfoAboutDriver), new { id = createdDriver.Id }, createdDriver);
    }

    /// <summary>
    /// Оновлення інформації про водія за Id
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateDriverAsync(
        int id,
        [FromBody] MiniDriverReqDTO drv,
        [FromServices] IValidator<MiniDriverReqDTO> validator,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0 || (drv.id != 0 && id != drv.id))
        {
            return BadRequest($"Некоректний ідентифікатор: {id}");
        }

        ValidationResult validationResult = await validator.ValidateAsync(drv, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var updated = await _driversManager.UpdateDriverInSystemAsync(id, drv, provider, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Видалення водія за Id
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteDriverByIdAsync(
        int id,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest($"Некоректний ідентифікатор: {id}");
        }

        await _driversManager.DeleteDriverFromSystemAsync(id, provider, cancellationToken);
        return Ok(new { Message = $"Водія з Id {id} успішно видалено." });
    }

    /// <summary>
    /// БІЗНЕС-ЛОГІКА 1: Комплексна перевірка комплаєнсу, допуску водія й тягача, калькуляція бюджету та реєстрація рейсу
    /// </summary>
    /// <remarks>
    /// Демонструє багатокроковий бізнес-процес:
    /// - Перевірка ліцензії водія, віку, ADR-допуску, медичної придатності
    /// - Перевірка технічного стану тягача та свіжості техогляду (ТО)
    /// - Багаторівневий тарифний калькулятор (паливо, добові, надбавки за стаж і небезпеку)
    /// - Автоматичне бронювання коштів у журналі витрат компанії
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("assess-and-dispatch")]
    [ProducesResponseType(typeof(DriverTripDispatchAssessmentDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DriverTripDispatchAssessmentDTO>> AssessAndDispatchDriverForTripAsync(
        [FromBody] DriverTripDispatchRequestDTO request,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            return BadRequest("Дані запиту на рейс відсутні.");
        }

        var result = await _driversManager.AssessAndDispatchDriverForTripAsync(request, provider, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// БІЗНЕС-ЛОГІКА 2: Річний операційний та безпековий аудит водія (KPI Scoring, грейдування та нарахування премії)
    /// </summary>
    /// <remarks>
    /// Демонструє бізнес-аналітику та оцінювання персоналу:
    /// - Агрегація річних витрат (паливо, ремонти, штрафи)
    /// - Нормалізовані показники KPI (паливна ефективність, дбайливість до авто, безпека)
    /// - Зважений скоринг (35% паливо + 35% техніка + 30% безпека)
    /// - Грейдування (A/B/C/UnderReview) та виплата стимулюючого річного бонусу
    /// </remarks>
    [AllowAnonymous]
    [HttpGet("{id}/annual-audit")]
    [ProducesResponseType(typeof(DriverAnnualAuditReportDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DriverAnnualAuditReportDTO>> ConductDriverAnnualPerformanceAuditAsync(
        int id,
        [FromQuery] int auditYear,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest($"Некоректний ідентифікатор водія: {id}");
        }

        if (auditYear < 2000 || auditYear > DateTime.UtcNow.Year + 1)
        {
            return BadRequest($"Некоректний рік аудиту: {auditYear}");
        }

        var result = await _driversManager.ConductDriverAnnualPerformanceAuditAsync(id, auditYear, provider, cancellationToken);
        return Ok(result);
    }
}
