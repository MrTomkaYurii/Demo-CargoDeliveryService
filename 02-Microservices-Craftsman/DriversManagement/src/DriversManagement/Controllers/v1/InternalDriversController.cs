namespace DriversManagement.Controllers.v1;

using DriversManagement.Databases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// ====================================================================================
/// 🔒 ВНУТРІШНІЙ (SERVICE-TO-SERVICE) API ВОДІЇВ
/// ------------------------------------------------------------------------------------
/// ПРИЗНАЧЕННЯ:
/// Компактна "картка водія" для CargoDelivery.HttpAggregator. Агрегатору не потрібне
/// повне досьє (паспорт, адреса, медогляд) — лише ПІБ, посвідчення, компанія та статус.
/// Це і є принцип "мінімально необхідних даних" (захист від Over-fetching).
///
/// ЧОМУ НЕ ЗВИЧАЙНИЙ /api/drivers/{id}?
/// Публічний DriversController вимагає JWT користувача ([Authorize] + HeimGuard), а
/// агрегатор викликає сервіс від власного імені, без токена користувача → було б 401.
///
/// БЕЗПЕКА:
/// [AllowAnonymous] — спрощення навчального стенду Aspire. У продакшні внутрішні API
/// закривають мережевими політиками, mTLS або OAuth2 Client Credentials.
/// ====================================================================================
/// </summary>
[ApiController]
[Route("api/internal/drivers")]
[ApiVersion("1.0")]
[AllowAnonymous]
public sealed class InternalDriversController : ControllerBase
{
    private readonly DriversManagementContext _db;

    public InternalDriversController(DriversManagementContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Повертає картку водія або 404, якщо водія не існує.
    /// 404 — це очікуваний бізнес-результат, а не збій: агрегатор обробляє його без винятку.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var driver = await _db.Drivers
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => new
            {
                d.Id,
                FullName = (d.FirstName + " " + d.LastName).Trim(),
                LicenseNumber = d.DriverLicenseNumber,
                CompanyName = d.Company != null ? d.Company.CompanyName : null,
                Status = d.EmploymentStatus
            })
            .FirstOrDefaultAsync(cancellationToken);

        return driver is null ? NotFound() : Ok(driver);
    }
}
