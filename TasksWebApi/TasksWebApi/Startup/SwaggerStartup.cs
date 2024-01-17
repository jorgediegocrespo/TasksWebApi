using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TasksWebApi.Startup;

public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureNamedOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions.Distinct())
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
    }

    public void Configure(string name, SwaggerGenOptions options)
    {
        Configure(options);
    }

    private OpenApiInfo CreateVersionInfo(ApiVersionDescription desc)
    {
        var info = new OpenApiInfo()
        {
            Title = "Tasks Web API",
            Version = desc.ApiVersion.ToString()
        };

        if (desc.IsDeprecated)
        {
            info.Description += " This API version has been deprecated. Please use one of the new APIs available from the explorer.";
        }

        return info;
    }
}

public static class SwaggerStartup
{
    public static void ConfigureSwaggerOptions(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            //API Key
            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "ApiKey must appear in header",
                Type = SecuritySchemeType.ApiKey,
                Name = "x-api-key",
                In = ParameterLocation.Header,
                Scheme = "ApiKeyScheme"
            });
            var key = new OpenApiSecurityScheme()
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = ParameterLocation.Header
            };
            var requirement = new OpenApiSecurityRequirement
            {
                { key, new List<string>() }
            };
            c.AddSecurityRequirement(requirement);

            //JWT
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please insert JWT with Bearer into field",
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
        });

        services.ConfigureOptions<ConfigureSwaggerOptions>();
    }
    
    public static void CustomUseSwagger(this IApplicationBuilder app)
    {
        var apiVersionDescriptionProvider = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Distinct().Reverse())
            {
                c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            }
        });
    }
}