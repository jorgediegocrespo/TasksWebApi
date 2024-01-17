using System.Security.Claims;
using TasksWebApi.Models;

namespace TasksWebApi.Services;

public class HttpContextService(IHttpContextAccessor httpContextAccessor) : IHttpContextService
{
    public UserResponse GetContextUser()
    {
        return new UserResponse(
            httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
            httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name),
            httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email),
            httpContextAccessor.HttpContext.User.FindAll(ClaimTypes.Role)?.Select(c => c.Value)?.ToList());
    }
    
    public string GetClientIp()
    {
        return httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
    }
}