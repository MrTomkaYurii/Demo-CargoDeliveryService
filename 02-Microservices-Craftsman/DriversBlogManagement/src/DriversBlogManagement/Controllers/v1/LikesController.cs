namespace DriversBlogManagement.Controllers.v1;

using DriversBlogManagement.Domain.Likes.Features;
using DriversBlogManagement.Domain.Likes.Dtos;
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
[Route("api/likes")]
[ApiVersion("1.0")]
public sealed class LikesController: ControllerBase
{
    private readonly IMediator _mediator;

    public LikesController(IMediator mediator)
    {
        _mediator = mediator;
    }
    

    /// <summary>
    /// Gets a list of all Likes.
    /// </summary>
    [Authorize]
    [HttpGet(Name = "GetLikes")]
    public async Task<IActionResult> GetLikes([FromQuery] LikeParametersDto likeParametersDto)
    {
        var query = new GetLikeList.Query(likeParametersDto);
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
    /// Gets a list of all Likes.
    /// </summary>
    [Authorize]
    [HttpGet("all", Name = "GetAllLikes")]
    public async Task<IActionResult> GetAllLikes()
    {
        var query = new GetAllLikes.Query();
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a single Like by ID.
    /// </summary>
    [Authorize]
    [HttpGet("{likeId:guid}", Name = "GetLike")]
    public async Task<ActionResult<LikeDto>> GetLike(Guid likeId)
    {
        var query = new GetLike.Query(likeId);
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Creates a new Like record.
    /// </summary>
    [Authorize]
    [HttpPost(Name = "AddLike")]
    public async Task<ActionResult<LikeDto>> AddLike([FromBody]LikeForCreationDto likeForCreation)
    {
        var command = new AddLike.Command(likeForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetLike",
            new { likeId = commandResponse.Id },
            commandResponse);
    }


    /// <summary>
    /// Updates an entire existing Like.
    /// </summary>
    [Authorize]
    [HttpPut("{likeId:guid}", Name = "UpdateLike")]
    public async Task<IActionResult> UpdateLike(Guid likeId, LikeForUpdateDto like)
    {
        var command = new UpdateLike.Command(likeId, like);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Deletes an existing Like record.
    /// </summary>
    [Authorize]
    [HttpDelete("{likeId:guid}", Name = "DeleteLike")]
    public async Task<ActionResult> DeleteLike(Guid likeId)
    {
        var command = new DeleteLike.Command(likeId);
        await _mediator.Send(command);
        return NoContent();
    }

    // endpoint marker - do not delete this comment
}
