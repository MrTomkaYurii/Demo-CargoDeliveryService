namespace CargoOrderingService.Controllers.v1;

using CargoOrderingService.Domain.Deliveries.Features;
using CargoOrderingService.Domain.Deliveries.Dtos;
using CargoOrderingService.Resources;
using CargoOrderingService.Domain;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using System.Threading;
using MediatR;

[ApiController]
[Route("api/deliveries")]
[ApiVersion("1.0")]
public sealed class DeliveriesController: ControllerBase
{
    private readonly IMediator _mediator;

    public DeliveriesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    

    /// <summary>
    /// Gets a list of all Deliveries.
    /// </summary>
    [Authorize]
    [HttpGet(Name = "GetDeliveries")]
    public async Task<IActionResult> GetDeliveries([FromQuery] DeliveryParametersDto deliveryParametersDto)
    {
        var query = new GetDeliveryList.Query(deliveryParametersDto);
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
    /// Gets a list of all Deliveries.
    /// </summary>
    [Authorize]
    [HttpGet("all", Name = "GetAllDeliveries")]
    public async Task<IActionResult> GetAllDeliveries()
    {
        var query = new GetAllDeliveries.Query();
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a single Delivery by ID.
    /// </summary>
    [Authorize]
    [HttpGet("{deliveryId:guid}", Name = "GetDelivery")]
    public async Task<ActionResult<DeliveryDto>> GetDelivery(Guid deliveryId)
    {
        var query = new GetDelivery.Query(deliveryId);
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Creates a new Delivery record.
    /// </summary>
    [Authorize]
    [HttpPost(Name = "AddDelivery")]
    public async Task<ActionResult<DeliveryDto>> AddDelivery([FromBody]DeliveryForCreationDto deliveryForCreation)
    {
        var command = new AddDelivery.Command(deliveryForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetDelivery",
            new { deliveryId = commandResponse.Id },
            commandResponse);
    }


    /// <summary>
    /// Updates an entire existing Delivery.
    /// </summary>
    [Authorize]
    [HttpPut("{deliveryId:guid}", Name = "UpdateDelivery")]
    public async Task<IActionResult> UpdateDelivery(Guid deliveryId, DeliveryForUpdateDto delivery)
    {
        var command = new UpdateDelivery.Command(deliveryId, delivery);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Deletes an existing Delivery record.
    /// </summary>
    [Authorize]
    [HttpDelete("{deliveryId:guid}", Name = "DeleteDelivery")]
    public async Task<ActionResult> DeleteDelivery(Guid deliveryId)
    {
        var command = new DeleteDelivery.Command(deliveryId);
        await _mediator.Send(command);
        return NoContent();
    }

    // endpoint marker - do not delete this comment
}
