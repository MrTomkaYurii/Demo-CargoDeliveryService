namespace DriversBlogManagement.Controllers.v1;

using DriversBlogManagement.Domain.BlogUsers.Features;
using DriversBlogManagement.Domain.BlogUsers.Dtos;
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
[Route("api/blogusers")]
[ApiVersion("1.0")]
public sealed class BlogUsersController: ControllerBase
{
    private readonly IMediator _mediator;

    public BlogUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }
    

    /// <summary>
    /// Gets a list of all BlogUsers.
    /// </summary>
    [Authorize]
    [HttpGet(Name = "GetBlogUsers")]
    public async Task<IActionResult> GetBlogUsers([FromQuery] BlogUserParametersDto blogUserParametersDto)
    {
        var query = new GetBlogUserList.Query(blogUserParametersDto);
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
    /// Gets a list of all BlogUsers.
    /// </summary>
    [Authorize]
    [HttpGet("all", Name = "GetAllBlogUsers")]
    public async Task<IActionResult> GetAllBlogUsers()
    {
        var query = new GetAllBlogUsers.Query();
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Gets a single BlogUser by ID.
    /// </summary>
    [Authorize]
    [HttpGet("{blogUserId:guid}", Name = "GetBlogUser")]
    public async Task<ActionResult<BlogUserDto>> GetBlogUser(Guid blogUserId)
    {
        var query = new GetBlogUser.Query(blogUserId);
        var queryResponse = await _mediator.Send(query);
        return Ok(queryResponse);
    }


    /// <summary>
    /// Creates a new BlogUser record.
    /// </summary>
    [Authorize]
    [HttpPost(Name = "AddBlogUser")]
    public async Task<ActionResult<BlogUserDto>> AddBlogUser([FromBody]BlogUserForCreationDto blogUserForCreation)
    {
        var command = new AddBlogUser.Command(blogUserForCreation);
        var commandResponse = await _mediator.Send(command);

        return CreatedAtRoute("GetBlogUser",
            new { blogUserId = commandResponse.Id },
            commandResponse);
    }


    /// <summary>
    /// Updates an entire existing BlogUser.
    /// </summary>
    [Authorize]
    [HttpPut("{blogUserId:guid}", Name = "UpdateBlogUser")]
    public async Task<IActionResult> UpdateBlogUser(Guid blogUserId, BlogUserForUpdateDto blogUser)
    {
        var command = new UpdateBlogUser.Command(blogUserId, blogUser);
        await _mediator.Send(command);
        return NoContent();
    }


    /// <summary>
    /// Deletes an existing BlogUser record.
    /// </summary>
    [Authorize]
    [HttpDelete("{blogUserId:guid}", Name = "DeleteBlogUser")]
    public async Task<ActionResult> DeleteBlogUser(Guid blogUserId)
    {
        var command = new DeleteBlogUser.Command(blogUserId);
        await _mediator.Send(command);
        return NoContent();
    }

    // endpoint marker - do not delete this comment
}
