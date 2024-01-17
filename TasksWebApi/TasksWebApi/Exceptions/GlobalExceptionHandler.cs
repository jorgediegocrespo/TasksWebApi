using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TasksWebApi.Constants;
using TasksWebApi.Models;
using TasksWebApi.Services;

namespace TasksWebApi.Exceptions;

public sealed class GlobalExceptionHandler(IServiceScopeFactory serviceScopeFactory) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json";
        
        switch (exception)
        {
            case NotValidOperationException notValidItemException:
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
                var customError = new CustomError(notValidItemException.Code, notValidItemException.Description);
                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(customError), cancellationToken: cancellationToken);
                break;
            }
            case DbUpdateConcurrencyException:
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
                var customError = new CustomError(ErrorCodes.CONCURRENCY_ERROR, "Concurrency error");
                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(customError), cancellationToken: cancellationToken);
                break;
            }
            case ForbidenActionException:
                httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                break;
            default:
            {
                using var scope = serviceScopeFactory.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<GlobalExceptionHandler>>();
                
                logger.LogError($"Unmanaged exception: {httpContext}");
                break;
            }
        }
        
        return true;
    }
}