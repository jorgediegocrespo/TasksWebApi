using System.Net;
using System.Text.Json;
using TasksWebApi.Exceptions;

namespace TasksWebApi.Middlewares;

public class ExceptionHandlerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode;
        string errorDescription;
        string errorCode;

        switch (exception)
        {
            case NotValidOperationException notValidItemException:
                statusCode = HttpStatusCode.Conflict;
                errorCode = notValidItemException.Code;
                errorDescription = notValidItemException.Description;
                break;
            default:
                statusCode = HttpStatusCode.InternalServerError;
                errorCode = "internal_server_error";
                errorDescription = exception.Message;
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var errorBody = JsonSerializer.Serialize(new { error_code = errorCode, error_description = errorDescription });
        return context.Response.WriteAsync(errorBody);
    }
}