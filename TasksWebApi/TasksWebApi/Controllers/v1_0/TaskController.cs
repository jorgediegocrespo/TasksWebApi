using Asp.Versioning;
using Audit.WebApi;
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
public class TaskController(ITaskService service) : BaseController
{
    [HttpPost]
    [Route("getAll")]
    [ProducesResponseType(typeof(PaginationResponse<ReadTaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [AuditIgnore]
    public async Task<IActionResult> GetAllAsync([FromBody]TaskPaginationRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest();

        var entities = await service.GetAllAsync(request, cancellationToken);
        return Ok(entities);
    }

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(ReadTaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AuditIgnore]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await service.GetAsync(id, cancellationToken);
        if (entity == null)
            return NotFound();
        
        return Ok(entity);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PostAsync([FromBody]CreateTaskRequest item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest();

        var createdId = await service.AddAsync(item, cancellationToken);
        return Created(nameof(GetByIdAsync), new { id = createdId });
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> PutAsync([FromBody]UpdateTaskRequest item, CancellationToken cancellationToken = default)
    {
        if (item == null)
            return BadRequest();

        if (!ModelState.IsValid)
            return BadRequest();

        await service.UpdateAsync(item, cancellationToken);
        return NoContent();
    }

    [HttpPut]
    [Route("delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAsync([FromBody]DeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        
        await service.DeleteAsync(deleteRequest, cancellationToken);
        return NoContent();
    }
    
    [HttpPut]
    [Route("deleteAll")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteAllAsync([FromBody]DeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest();
        
        await service.DeleteAllInListAsync(deleteRequest, cancellationToken);
        return NoContent();
    }
}