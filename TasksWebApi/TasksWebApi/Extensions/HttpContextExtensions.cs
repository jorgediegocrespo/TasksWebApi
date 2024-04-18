using System.Security.Claims;
using TasksWebApi.Models;

namespace TasksWebApi.Extensions;

public static class HttpContextExtensions
{
    public static UserResponse GetContextUser(this HttpContext httpContext)
    {
        return new UserResponse(
            httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
            httpContext.User.FindFirstValue(ClaimTypes.Name),
            httpContext.User.FindFirstValue(ClaimTypes.Email),
            httpContext.User.FindAll(ClaimTypes.Role)?.Select(c => c.Value)?.ToList());
    }
    
    public static string GetClientIp(this HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress.ToString();
    }
}