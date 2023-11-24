namespace TasksWebApi.Middlewares;

public static class WebApplicationExtensions
{
    public static void AddMiddlewares(this WebApplication app)
    {
        //app.UseWhen(context => !context.Request.Path.Equals("/api/test"), appBuilder => appBuilder.UseMiddleware<ApiKeyMiddleware>());
        //app.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}