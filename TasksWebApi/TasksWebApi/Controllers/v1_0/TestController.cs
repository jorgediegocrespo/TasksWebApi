using Asp.Versioning;
using Audit.WebApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TasksWebApi.Controllers.V1_0;

[AllowAnonymous]
[ApiVersion("1.0", Deprecated = true)]
[AuditIgnore]
public class TestController : BaseController
{
    [HttpGet]
    public IActionResult Test(CancellationToken cancellationToken = default)
    {
        return Ok("20231127");
    }
}
