using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TasksWebApi.Constants;
using TasksWebApi.Models;

namespace TasksWebApi.Exceptions;

public static class WebApplicationExtensions
{
    public static void ConfigureExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                //logger.LogError($"Something went wrong: {contextFeature.Error}");
                if (contextFeature?.Error is NotValidOperationException notValidItemException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    var customError = new CustomError(notValidItemException.Code, notValidItemException.Description);
                    await context.Response.WriteAsync(JsonSerializer.Serialize(customError));
                }
                else if (contextFeature?.Error is DbUpdateConcurrencyException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    var customError = new CustomError(ErrorCodes.CONCURRENCY_ERROR, "Concurrency error");
                    await context.Response.WriteAsync(JsonSerializer.Serialize(customError));
                }
                else if (contextFeature?.Error is ForbidenActionException)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
            });
        });
    }
}