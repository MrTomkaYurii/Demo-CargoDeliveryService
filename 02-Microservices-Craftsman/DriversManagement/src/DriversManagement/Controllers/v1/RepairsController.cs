namespace DriversManagement.Controllers.v1;

using DriversManagement.Domain.Repairs.Features;
using DriversManagement.Domain.Repairs.Dtos;
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
[Route("api/repairs")]
[ApiVersion("1.0")]
public sealed class RepairsController: ControllerBase
{
    private readonly IMediator _mediator;

    public RepairsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    

    /// <summary>
    /// Gets a list of all Repairs.
    /// </summary>
    [Authorize]
    [HttpGet(Name = "GetRepairs")]
    public async Task<IActionResult> GetRepairs([FromQuery] RepairParametersDto repairParametersDto)
    {
        var query = new GetRepairList.Query(repairParametersDto);
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
    /// Gets a list of all Repairs.
    /// </summary>
    [Authorize]
    [HttpGet("all", Name = "GetAllRepairs")]
    public async Task<IActionResult> GetAllRepairs()
    {
        var query = new GetAllRepairs.Query();
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a single Repair by ID.
    /// </summary>
    [Authorize]
    [HttpGet("{repairId:guid}", Name = "GetRepair")]
    public async Task<ActionResult<RepairDto>> GetRepair(Guid repairId)
    {
        var query = new GetRepair.Query(repairId);
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Creates a new Repair record.
    /// </summary>
    [Authorize]
    [HttpPost(Name = "AddRepair")]
    public async Task<ActionResult<RepairDto>> AddRepair([FromBody]RepairForCreationDto repairForCreation)
    {
        var command = new AddRepair.Command(repairForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetRepair",
            new { repairId = commandResponse.Id },
            commandResponse);
    }


    /// <summary>
    /// Updates an entire existing Repair.
    /// </summary>
    [Authorize]
    [HttpPut("{repairId:guid}", Name = "UpdateRepair")]
    public async Task<IActionResult> UpdateRepair(Guid repairId, RepairForUpdateDto repair)
    {
        var command = new UpdateRepair.Command(repairId, repair);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Deletes an existing Repair record.
    /// </summary>
    [Authorize]
    [HttpDelete("{repairId:guid}", Name = "DeleteRepair")]
    public async Task<ActionResult> DeleteRepair(Guid repairId)
    {
        var command = new DeleteRepair.Command(repairId);
        await _mediator.Send(command);
        return NoContent();
    }

    // endpoint marker - do not delete this comment
}
