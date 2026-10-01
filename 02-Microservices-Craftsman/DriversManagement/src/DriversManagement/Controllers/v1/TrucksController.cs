namespace DriversManagement.Controllers.v1;

using DriversManagement.Domain.Trucks.Features;
using DriversManagement.Domain.Trucks.Dtos;
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
[Route("api/trucks")]
[ApiVersion("1.0")]
public sealed class TrucksController: ControllerBase
{
    private readonly IMediator _mediator;

    public TrucksController(IMediator mediator)
    {
        _mediator = mediator;
    }
    

    /// <summary>
    /// Gets a list of all Trucks.
    /// </summary>
    [Authorize]
    [HttpGet(Name = "GetTrucks")]
    public async Task<IActionResult> GetTrucks([FromQuery] TruckParametersDto truckParametersDto)
    {
        var query = new GetTruckList.Query(truckParametersDto);
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
    /// Gets a list of all Trucks.
    /// </summary>
    [Authorize]
    [HttpGet("all", Name = "GetAllTrucks")]
    public async Task<IActionResult> GetAllTrucks()
    {
        var query = new GetAllTrucks.Query();
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a single Truck by ID.
    /// </summary>
    [Authorize]
    [HttpGet("{truckId:guid}", Name = "GetTruck")]
    public async Task<ActionResult<TruckDto>> GetTruck(Guid truckId)
    {
        var query = new GetTruck.Query(truckId);
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Creates a new Truck record.
    /// </summary>
    [Authorize]
    [HttpPost(Name = "AddTruck")]
    public async Task<ActionResult<TruckDto>> AddTruck([FromBody]TruckForCreationDto truckForCreation)
    {
        var command = new AddTruck.Command(truckForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetTruck",
            new { truckId = commandResponse.Id },
            commandResponse);
    }


    /// <summary>
    /// Updates an entire existing Truck.
    /// </summary>
    [Authorize]
    [HttpPut("{truckId:guid}", Name = "UpdateTruck")]
    public async Task<IActionResult> UpdateTruck(Guid truckId, TruckForUpdateDto truck)
    {
        var command = new UpdateTruck.Command(truckId, truck);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Deletes an existing Truck record.
    /// </summary>
    [Authorize]
    [HttpDelete("{truckId:guid}", Name = "DeleteTruck")]
    public async Task<ActionResult> DeleteTruck(Guid truckId)
    {
        var command = new DeleteTruck.Command(truckId);
        await _mediator.Send(command);
        return NoContent();
    }

    // endpoint marker - do not delete this comment
}
