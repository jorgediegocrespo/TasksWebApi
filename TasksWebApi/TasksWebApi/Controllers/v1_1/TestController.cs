using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TasksWebApi.Services;

namespace TasksWebApi.Controllers.V1_1;

[AllowAnonymous]
[ApiVersion("1.1")]
public class TestController(ICacheService cacheService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Test(CancellationToken cancellationToken = default)
    {
        int value = await cacheService.GetAsync("value", 0, cancellationToken);
        value++;
        await cacheService.SetAsync("value", value, DateTime.UtcNow.AddMinutes(20), cancellationToken);
        return Ok(value);
    }
    
    [HttpGet]
    [Route("test2")]
    public async Task<IActionResult> Test2(CancellationToken cancellationToken = default)
    {
        int value = await cacheService.GetAsync("value", 0, cancellationToken);
        return Ok(value);
    }
    
    [HttpGet]
    [Route("test3")]
    public async Task<IActionResult> Test3(CancellationToken cancellationToken = default)
    {
        await cacheService.RemoveAsync("value", cancellationToken);
        return Ok("ok");
    }
}