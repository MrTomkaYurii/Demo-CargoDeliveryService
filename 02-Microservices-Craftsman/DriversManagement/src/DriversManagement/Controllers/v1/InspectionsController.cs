namespace DriversManagement.Controllers.v1;

using DriversManagement.Domain.Inspections.Features;
using DriversManagement.Domain.Inspections.Dtos;
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
[Route("api/inspections")]
[ApiVersion("1.0")]
public sealed class InspectionsController: ControllerBase
{
    private readonly IMediator _mediator;

    public InspectionsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    

    /// <summary>
    /// Gets a list of all Inspections.
    /// </summary>
    [Authorize]
    [HttpGet(Name = "GetInspections")]
    public async Task<IActionResult> GetInspections([FromQuery] InspectionParametersDto inspectionParametersDto)
    {
        var query = new GetInspectionList.Query(inspectionParametersDto);
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
    /// Gets a list of all Inspections.
    /// </summary>
    [Authorize]
    [HttpGet("all", Name = "GetAllInspections")]
    public async Task<IActionResult> GetAllInspections()
    {
        var query = new GetAllInspections.Query();
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a single Inspection by ID.
    /// </summary>
    [Authorize]
    [HttpGet("{inspectionId:guid}", Name = "GetInspection")]
    public async Task<ActionResult<InspectionDto>> GetInspection(Guid inspectionId)
    {
        var query = new GetInspection.Query(inspectionId);
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Creates a new Inspection record.
    /// </summary>
    [Authorize]
    [HttpPost(Name = "AddInspection")]
    public async Task<ActionResult<InspectionDto>> AddInspection([FromBody]InspectionForCreationDto inspectionForCreation)
    {
        var command = new AddInspection.Command(inspectionForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetInspection",
            new { inspectionId = commandResponse.Id },
            commandResponse);
    }


    /// <summary>
    /// Updates an entire existing Inspection.
    /// </summary>
    [Authorize]
    [HttpPut("{inspectionId:guid}", Name = "UpdateInspection")]
    public async Task<IActionResult> UpdateInspection(Guid inspectionId, InspectionForUpdateDto inspection)
    {
        var command = new UpdateInspection.Command(inspectionId, inspection);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Deletes an existing Inspection record.
    /// </summary>
    [Authorize]
    [HttpDelete("{inspectionId:guid}", Name = "DeleteInspection")]
    public async Task<ActionResult> DeleteInspection(Guid inspectionId)
    {
        var command = new DeleteInspection.Command(inspectionId);
        await _mediator.Send(command);
        return NoContent();
    }

    // endpoint marker - do not delete this comment
}
