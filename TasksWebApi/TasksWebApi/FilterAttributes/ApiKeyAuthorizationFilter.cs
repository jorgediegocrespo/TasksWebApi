using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TasksWebApi.FilterAttributes;

public class ApiKeyAttribute : ServiceFilterAttribute
{
    public ApiKeyAttribute() : base(typeof(ApiKeyAuthorizationFilter))
    {
    }
}

public class ApiKeyAuthorizationFilter : IAuthorizationFilter
{
    private const string API_KEY_HEADER_NAME = "x-api-key";

    private readonly IConfiguration _configuration;

    public ApiKeyAuthorizationFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.ContainsKey(API_KEY_HEADER_NAME))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var headerValue = context.HttpContext.Request.Headers[API_KEY_HEADER_NAME];
        if (string.IsNullOrWhiteSpace(headerValue))
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        var apiKey = _configuration.GetValue<string>("XApiKey");
        if (apiKey is null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
     
        if (!apiKey.Equals(headerValue))
        {
            context.Result = new UnauthorizedResult();
            return;
        }
    }
}