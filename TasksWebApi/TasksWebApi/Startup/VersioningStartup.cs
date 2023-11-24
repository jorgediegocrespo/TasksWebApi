using Asp.Versioning;

namespace TasksWebApi.Startup;

public static class VersioningStartup
{
    public static IApiVersioningBuilder AddVersioning(this IServiceCollection services)
    {
        return services.AddApiVersioning(opt =>
            {
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.AssumeDefaultVersionWhenUnspecified = false;
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("x-api-version"),
                    new MediaTypeApiVersionReader("x-api-version"),
                    new UrlSegmentApiVersionReader());
            })
            .AddApiExplorer(setup =>
            {
                setup.GroupNameFormat = "'v'VVVV";
                setup.SubstituteApiVersionInUrl = true;
            });
    }
}