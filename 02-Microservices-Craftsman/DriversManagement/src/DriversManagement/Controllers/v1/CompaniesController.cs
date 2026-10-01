namespace DriversManagement.Controllers.v1;

using DriversManagement.Domain.Companies.Features;
using DriversManagement.Domain.Companies.Dtos;
using DriversManagement.Wrappers;
using DriversManagement.Domain;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using System.Threading;
using MediatR;

[ApiController]
[Route("api/companies")]
[ApiVersion("1.0")]
public sealed class CompaniesController: ControllerBase
{
    private readonly IMediator _mediator;

    public CompaniesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    

    /// <summary>
    /// Gets a list of all Companies.
    /// </summary>
    [Authorize]
    [HttpGet(Name = "GetCompanies")]
    public async Task<IActionResult> GetCompanies([FromQuery] CompanyParametersDto companyParametersDto)
    {
        var query = new GetCompanyList.Query(companyParametersDto);
        var queryResponse = await _mediator.Send(query);

        var paginationMetadata = new
        {
            totalCount = queryResponse.TotalCount,
            pageSize = queryResponse.PageSize,
            currentPageSize = queryResponse.CurrentPageSize,
            currentStartIndex = queryResponse.CurrentStartIndex,
            currentEndIndex = queryResponse.CurrentEndIndex,
            pageNumber = queryResponse.PageNumber,
            totalPages = queryResponse.TotalPages,
            hasPrevious = queryResponse.HasPrevious,
            hasNext = queryResponse.HasNext
        };

        Response.Headers.Add("X-Pagination",
            JsonSerializer.Serialize(paginationMetadata));

        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a list of all Companies.
    /// </summary>
    [Authorize]
    [HttpGet("all", Name = "GetAllCompanies")]
    public async Task<IActionResult> GetAllCompanies()
    {
        var query = new GetAllCompanies.Query();
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a single Company by ID.
    /// </summary>
    [Authorize]
    [HttpGet("{companyId:guid}", Name = "GetCompany")]
    public async Task<ActionResult<CompanyDto>> GetCompany(Guid companyId)
    {
        var query = new GetCompany.Query(companyId);
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Creates a new Company record.
    /// </summary>
    [Authorize]
    [HttpPost(Name = "AddCompany")]
    public async Task<ActionResult<CompanyDto>> AddCompany([FromBody]CompanyForCreationDto companyForCreation)
    {
        var command = new AddCompany.Command(companyForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetCompany",
            new { companyId = commandResponse.Id },
            commandResponse);
    }


    /// <summary>
    /// Updates an entire existing Company.
    /// </summary>
    [Authorize]
    [HttpPut("{companyId:guid}", Name = "UpdateCompany")]
    public async Task<IActionResult> UpdateCompany(Guid companyId, CompanyForUpdateDto company)
    {
        var command = new UpdateCompany.Command(companyId, company);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Deletes an existing Company record.
    /// </summary>
    [Authorize]
    [HttpDelete("{companyId:guid}", Name = "DeleteCompany")]
    public async Task<ActionResult> DeleteCompany(Guid companyId)
    {
        var command = new DeleteCompany.Command(companyId);
        await _mediator.Send(command);
        return NoContent();
    }

    // endpoint marker - do not delete this comment
}
