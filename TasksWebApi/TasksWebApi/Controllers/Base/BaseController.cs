using Microsoft.AspNetCore.Mvc;

namespace TasksWebApi.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected BaseController()
    {
    }
}