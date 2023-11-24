using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TasksWebApi.FilterAttributes;
using TasksWebApi.Models;
using TasksWebApi.Services;

namespace TasksWebApi.Controllers.V1_0;

[ApiVersion("1.0")]
[ApiKey]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class TaskListController : BaseController
{
    private readonly ITaskListService _service;
    
    public TaskListController(ITaskListService service)
    {
        _service = service;
    }

    [HttpPost]
    [Route("getAll")]
    [ProducesResponseType(typeof(PaginationResponse<ReadTaskListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllAsync([FromBody]PaginationRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest();

        var entities = await _service.GetAllAsync(request, cancellationToken);
        return Ok(entities);
    }

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(ReadTaskListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _service.GetAsync(id, cancellationToken);
        if (entity == null)
            return NotFound();
        
        return Ok(entity);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostAsync([FromBody]CreateTaskListRequest item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest();

        int createdId = await _service.AddAsync(item, cancellationToken);
        return Created(nameof(GetByIdAsync), new { id = createdId });
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PutAsync([FromBody]UpdateTaskListRequest item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest();

        await _service.UpdateAsync(item, cancellationToken);
        return NoContent();
    }

    [HttpPut]
    [Route("delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAsync([FromBody] DeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        
        await _service.DeleteAsync(deleteRequest, cancellationToken);
        return NoContent();
    }
}