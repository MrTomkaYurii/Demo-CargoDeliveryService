namespace DriversManagement.Controllers.v1;

using DriversManagement.Domain.Photos.Features;
using DriversManagement.Domain.Photos.Dtos;
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
[Route("api/photos")]
[ApiVersion("1.0")]
public sealed class PhotosController: ControllerBase
{
    private readonly IMediator _mediator;

    public PhotosController(IMediator mediator)
    {
        _mediator = mediator;
    }
    

    /// <summary>
    /// Gets a list of all Photos.
    /// </summary>
    [Authorize]
    [HttpGet(Name = "GetPhotos")]
    public async Task<IActionResult> GetPhotos([FromQuery] PhotoParametersDto photoParametersDto)
    {
        var query = new GetPhotoList.Query(photoParametersDto);
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
    /// Gets a list of all Photos.
    /// </summary>
    [Authorize]
    [HttpGet("all", Name = "GetAllPhotos")]
    public async Task<IActionResult> GetAllPhotos()
    {
        var query = new GetAllPhotos.Query();
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a single Photo by ID.
    /// </summary>
    [Authorize]
    [HttpGet("{photoId:guid}", Name = "GetPhoto")]
    public async Task<ActionResult<PhotoDto>> GetPhoto(Guid photoId)
    {
        var query = new GetPhoto.Query(photoId);
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Creates a new Photo record.
    /// </summary>
    [Authorize]
    [HttpPost(Name = "AddPhoto")]
    public async Task<ActionResult<PhotoDto>> AddPhoto([FromBody]PhotoForCreationDto photoForCreation)
    {
        var command = new AddPhoto.Command(photoForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetPhoto",
            new { photoId = commandResponse.Id },
            commandResponse);
    }


    /// <summary>
    /// Updates an entire existing Photo.
    /// </summary>
    [Authorize]
    [HttpPut("{photoId:guid}", Name = "UpdatePhoto")]
    public async Task<IActionResult> UpdatePhoto(Guid photoId, PhotoForUpdateDto photo)
    {
        var command = new UpdatePhoto.Command(photoId, photo);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Deletes an existing Photo record.
    /// </summary>
    [Authorize]
    [HttpDelete("{photoId:guid}", Name = "DeletePhoto")]
    public async Task<ActionResult> DeletePhoto(Guid photoId)
    {
        var command = new DeletePhoto.Command(photoId);
        await _mediator.Send(command);
        return NoContent();
    }

    // endpoint marker - do not delete this comment
}
