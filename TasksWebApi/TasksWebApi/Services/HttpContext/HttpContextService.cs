using TasksWebApi.Extensions;
using TasksWebApi.Models;

namespace TasksWebApi.Services;

public class HttpContextService(IHttpContextAccessor httpContextAccessor) : IHttpContextService
{
    public UserResponse GetContextUser() => httpContextAccessor.HttpContext.GetContextUser();
    
    public string GetClientIp() => httpContextAccessor.HttpContext.GetClientIp();
}