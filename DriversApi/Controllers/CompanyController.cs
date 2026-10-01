using Drivers.BLL.DTOs.Requests;
using Drivers.BLL.DTOs.Responses;
using Drivers.BLL.Managers.Contracts;
using Drivers.BLL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivers.Api.Controllers;

/// <summary>
/// Контролер управління компаніями-перевізниками.
/// Підтримує повний REST CRUD та вибір провайдера даних (EF Core або Dapper).
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CompanyController : ControllerBase
{
    private readonly ILogger<CompanyController> _logger;
    private readonly ICompanyManager _companyManager;

    public CompanyController(
        ILogger<CompanyController> logger,
        ICompanyManager companyManager)
    {
        _logger = logger;
        _companyManager = companyManager;
    }

    /// <summary>
    /// Отримання списку всіх компаній
    /// </summary>
    [AllowAnonymous]

    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CompanyResponceDTO>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CompanyResponceDTO>>> GetAll(
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var companies = await _companyManager.GetAllCompaniesAsync(provider, cancellationToken);
        return Ok(companies);
    }

    /// <summary>
    /// Отримання інформації про компанію за ідентифікатором
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CompanyResponceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponceDTO>> GetById(
        int id,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var company = await _companyManager.GetCompanyByIdAsync(id, provider, cancellationToken);
        if (company == null)
        {
            _logger.LogWarning("Компанію з Id {Id} не знайдено", id);
            return NotFound($"Компанію з Id {id} не знайдено.");
        }
        return Ok(company);
    }

    /// <summary>
    /// Отримання компанії разом із закріпленими водіями
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}/drivers")]
    [ProducesResponseType(typeof(CompanyWithDriversResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyWithDriversResponseDTO>> GetWithDrivers(
        int id,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var company = await _companyManager.GetCompanyWithDriversAsync(id, provider, cancellationToken);
        if (company == null)
        {
            return NotFound($"Компанію з Id {id} не знайдено.");
        }
        return Ok(company);
    }

    /// <summary>
    /// Створення нової компанії
    /// </summary>
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(typeof(CompanyResponceDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CompanyResponceDTO>> Create(
        [FromBody] CompanyReqDTO dto,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _companyManager.CreateCompanyAsync(dto, provider, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Оновлення даних компанії
    /// </summary>
    [AllowAnonymous]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(CompanyResponceDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompanyResponceDTO>> Update(
        int id,
        [FromBody] CompanyReqDTO dto,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _companyManager.UpdateCompanyAsync(id, dto, provider, cancellationToken);
        if (updated == null)
        {
            return NotFound($"Компанію з Id {id} не знайдено для оновлення.");
        }
        return Ok(updated);
    }

    /// <summary>
    /// Видалення компанії за ідентифікатором
    /// </summary>
    [AllowAnonymous]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        [FromQuery] DataAccessProvider provider = DataAccessProvider.EntityFramework,
        CancellationToken cancellationToken = default)
    {
        var deleted = await _companyManager.DeleteCompanyAsync(id, provider, cancellationToken);
        if (!deleted)
        {
            return NotFound($"Компанію з Id {id} не знайдено для видалення.");
        }
        return NoContent();
    }
}
