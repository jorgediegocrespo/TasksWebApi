using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TasksWebApi.DataAccess;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Repositories;
using TasksWebApi.FilterAttributes;
using TasksWebApi.Services;

namespace TasksWebApi.Startup;

public static class CustomServicesStartup
{
    public static void RegisterConfigurationValuesService(this IServiceCollection services, IConfiguration configuration)
    {
        var useVault = configuration.GetValue<bool>("ConfigurationValues:UseVault");
        if (useVault)
        {
            services.Configure<VaultSettings>(configuration);
            services.PostConfigure<VaultSettings>(settings => settings.UpdateUrl(configuration.GetValue<string>("ConfigurationValues:VaultUrl")));
            services.AddSingleton<IConfigurationValuesService, VaultConfigurationValuesService>();
        }
        else
        {
            services.AddSingleton<IConfigurationValuesService, AppSettingsConfigurationValuesService>();
        }
    }

    public static void RegisterDbContext(this WebApplicationBuilder builder)
    {
        var configurationValues = builder.Services.BuildServiceProvider().GetService<IConfigurationValuesService>();
        var connectionString = configurationValues.GetDataBaseConnectionAsync().Result;
        builder.Services.AddDbContext<TasksDbContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
    }
    
    public static void AddRegistrations(this IServiceCollection services)
    {
        RegisterFilters(services);
        RegisterIdentity(services);
        RegisterRepositories(services);
        RegisterServices(services);
        RegisterOthers(services);
    }

    private static void RegisterFilters(IServiceCollection services)
    {
        services.AddSingleton<ApiKeyAuthorizationFilter>();
    }
    
    private static void RegisterIdentity(IServiceCollection services)
    {
        services.AddIdentity<UserEntity, IdentityRole>(o =>
            {
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireUppercase = true;
                o.Password.RequireNonAlphanumeric = true;
                o.Password.RequiredLength = 12;
                o.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<TasksDbContext>()
            .AddDefaultTokenProviders();
    }

    private static void RegisterRepositories(IServiceCollection services)
    {
        services.AddTransient<ITaskRepository, TaskRepository>();
        services.AddTransient<ITaskListRepository, TaskListRepository>();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.AddScoped<IHttpContextService, HttpContextService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<ITaskListService, TaskListService>();
        services.AddTransient<ITaskService, TaskService>();
    }

    private static void RegisterOthers(IServiceCollection services)
    {
        
    }
}