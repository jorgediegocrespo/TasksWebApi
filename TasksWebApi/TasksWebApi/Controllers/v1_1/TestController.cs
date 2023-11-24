using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TasksWebApi.Controllers.V1_1;

[AllowAnonymous]
[ApiVersion("1.1")]
public class TestController : BaseController
{
    [HttpGet]
    public IActionResult Test(CancellationToken cancellationToken = default)
    {
        return Ok("v1.1");
    }
}