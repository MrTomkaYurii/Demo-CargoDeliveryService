namespace DriversManagement.Controllers.v1;

using DriversManagement.Domain.Drivers.Features;
using DriversManagement.Domain.Drivers.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;

/// <summary>
/// ====================================================================================
/// 🌐 API GATEWAY AGGREGATOR / BACKEND FOR FRONTEND (BFF) CONTROLLER
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Цей контролер надає спеціалізовані композитні (агреговані) ендпоінти, оптимізовані
/// для потреб клієнтського інтерфейсу (Web SPA, мобільні додатки, WPF десктоп-клієнт).
///
/// ЧОМУ ЦЕ ВИНЕСЕНО В ОКРЕМИЙ КОНТРОЛЕР АГРЕГАТОРА?
/// 1. Розділення відповідальності:
///    Звичайний DriversController фокусується на CRUD-операціях суто над локальною
///    сутністю Driver (створити, оновити, видалити).
///    Агрегатор (DriverDashboardController) призначений для читання та компонування
///    даних з кількох доменів і сервісів.
/// 2. Оптимізація трафіку (BFF):
///    Клієнт отримує строго ті поля, які потрібні для відображення картки водія,
///    без зайвого надлишкового payload і без потреби робити 3 окремих запити.
/// ====================================================================================
/// </summary>
[ApiController]
[Route("api/v1/driver-dashboard")]
[ApiVersion("1.0")]
public sealed class DriverDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DriverDashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Отримує зведену аналітичну панель водія (дані профілю, авто, активні рейси та публікації).
    /// </summary>
    /// <param name="id">Ідентифікатор водія (Guid).</param>
    /// <param name="cancellationToken">Токен скасування запиту.</param>
    /// <returns>Зведена модель AggregatedDriverDashboardDto.</returns>
    /// <response code="200">Успішно сформовано дашборд (повні або часткові дані).</response>
    /// <response code="404">Водія не знайдено в локальній базі даних.</response>
    [AllowAnonymous] // Для демонстрації тестування дозволяємо виклик дашборду
    [HttpGet("{id:guid}", Name = "GetDriverDashboard")]
    [ProducesResponseType(typeof(AggregatedDriverDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AggregatedDriverDashboardDto>> GetDashboard(
        [FromRoute] Guid id, 
        CancellationToken cancellationToken)
    {
        var query = new GetDriverDashboard.Query(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
