namespace DriversManagement.Controllers.v1;

using DriversManagement.Domain.Expences.Features;
using DriversManagement.Domain.Expences.Dtos;
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
[Route("api/expences")]
[ApiVersion("1.0")]
public sealed class ExpencesController: ControllerBase
{
    private readonly IMediator _mediator;

    public ExpencesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    

    /// <summary>
    /// Gets a list of all Expences.
    /// </summary>
    [Authorize]
    [HttpGet(Name = "GetExpences")]
    public async Task<IActionResult> GetExpences([FromQuery] ExpenceParametersDto expenceParametersDto)
    {
        var query = new GetExpenceList.Query(expenceParametersDto);
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
    /// Gets a list of all Expences.
    /// </summary>
    [Authorize]
    [HttpGet("all", Name = "GetAllExpences")]
    public async Task<IActionResult> GetAllExpences()
    {
        var query = new GetAllExpences.Query();
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a single Expence by ID.
    /// </summary>
    [Authorize]
    [HttpGet("{expenceId:guid}", Name = "GetExpence")]
    public async Task<ActionResult<ExpenceDto>> GetExpence(Guid expenceId)
    {
        var query = new GetExpence.Query(expenceId);
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Creates a new Expence record.
    /// </summary>
    [Authorize]
    [HttpPost(Name = "AddExpence")]
    public async Task<ActionResult<ExpenceDto>> AddExpence([FromBody]ExpenceForCreationDto expenceForCreation)
    {
        var command = new AddExpence.Command(expenceForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetExpence",
            new { expenceId = commandResponse.Id },
            commandResponse);
    }


    /// <summary>
    /// Updates an entire existing Expence.
    /// </summary>
    [Authorize]
    [HttpPut("{expenceId:guid}", Name = "UpdateExpence")]
    public async Task<IActionResult> UpdateExpence(Guid expenceId, ExpenceForUpdateDto expence)
    {
        var command = new UpdateExpence.Command(expenceId, expence);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Deletes an existing Expence record.
    /// </summary>
    [Authorize]
    [HttpDelete("{expenceId:guid}", Name = "DeleteExpence")]
    public async Task<ActionResult> DeleteExpence(Guid expenceId)
    {
        var command = new DeleteExpence.Command(expenceId);
        await _mediator.Send(command);
        return NoContent();
    }

    // endpoint marker - do not delete this comment
}
