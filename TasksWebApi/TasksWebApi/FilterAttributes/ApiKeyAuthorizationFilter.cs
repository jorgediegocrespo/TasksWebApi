using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TasksWebApi.Services;

namespace TasksWebApi.FilterAttributes;

public class ApiKeyAttribute() : ServiceFilterAttribute(typeof(ApiKeyAuthorizationFilter));

public class ApiKeyAuthorizationFilter(IConfigurationValuesService configurationValuesService) : IAuthorizationFilter
{
    private const string API_KEY_HEADER_NAME = "x-api-key";

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
        
        var apiKey = configurationValuesService.GetXApiKey().Result;
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