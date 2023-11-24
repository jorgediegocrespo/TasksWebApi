using System.Security.Claims;
using TasksWebApi.Models;

namespace TasksWebApi.Services;

public class HttpContextService : IHttpContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public UserResponse GetContextUser()
    {
        return new UserResponse(
            _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
            _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name),
            _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email),
            _httpContextAccessor.HttpContext.User.FindAll(ClaimTypes.Role)?.Select(c => c.Value)?.ToList());
    }
}