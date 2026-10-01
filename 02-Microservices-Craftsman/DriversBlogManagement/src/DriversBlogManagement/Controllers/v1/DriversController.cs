namespace DriversBlogManagement.Controllers.v1;

using DriversBlogManagement.Domain.Drivers.Features;
using DriversBlogManagement.Domain.Drivers.Dtos;
using DriversBlogManagement.Wrappers;
using DriversBlogManagement.Domain;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using System.Threading;
using MediatR;

[ApiController]
[Route("api/drivers")]
[ApiVersion("1.0")]
public sealed class DriversController: ControllerBase
{
    private readonly IMediator _mediator;

    public DriversController(IMediator mediator)
    {
        _mediator = mediator;
    }
    

    /// <summary>
    /// Gets a list of all Drivers.
    /// </summary>
    [Authorize]
    [HttpGet(Name = "GetDrivers")]
    public async Task<IActionResult> GetDrivers([FromQuery] DriverParametersDto driverParametersDto)
    {
        var query = new GetDriverList.Query(driverParametersDto);
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
    /// Gets a list of all Drivers.
    /// </summary>
    [Authorize]
    [HttpGet("all", Name = "GetAllDrivers")]
    public async Task<IActionResult> GetAllDrivers()
    {
        var query = new GetAllDrivers.Query();
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a single Driver by ID.
    /// </summary>
    [Authorize]
    [HttpGet("{driverId:guid}", Name = "GetDriver")]
    public async Task<ActionResult<DriverDto>> GetDriver(Guid driverId)
    {
        var query = new GetDriver.Query(driverId);
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Creates a new Driver record.
    /// </summary>
    [Authorize]
    [HttpPost(Name = "AddDriver")]
    public async Task<ActionResult<DriverDto>> AddDriver([FromBody]DriverForCreationDto driverForCreation)
    {
        var command = new AddDriver.Command(driverForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetDriver",
            new { driverId = commandResponse.Id },
            commandResponse);
    }


    /// <summary>
    /// Updates an entire existing Driver.
    /// </summary>
    [Authorize]
    [HttpPut("{driverId:guid}", Name = "UpdateDriver")]
    public async Task<IActionResult> UpdateDriver(Guid driverId, DriverForUpdateDto driver)
    {
        var command = new UpdateDriver.Command(driverId, driver);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Deletes an existing Driver record.
    /// </summary>
    [Authorize]
    [HttpDelete("{driverId:guid}", Name = "DeleteDriver")]
    public async Task<ActionResult> DeleteDriver(Guid driverId)
    {
        var command = new DeleteDriver.Command(driverId);
        await _mediator.Send(command);
        return NoContent();
    }

    // endpoint marker - do not delete this comment
}
