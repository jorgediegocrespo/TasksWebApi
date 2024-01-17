using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TasksWebApi.DataAccess;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Repositories;
using TasksWebApi.FilterAttributes;
using TasksWebApi.Services;

namespace TasksWebApi.Startup;

public static class CustomServicesStartup
{
    public static void AddRegistrations(this IServiceCollection services)
    {
        RegisterFilters(services);
        RegisterIdentity(services);
        RegisterRepositories(services);
        RegisterServices(services);
        RegisterOthers(services);
    }
    
    public static void RegisterDbContext(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DataBaseConnection");
        builder.Services.AddDbContext<TasksDbContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
    }

    private static void RegisterFilters(IServiceCollection services)
    {
        services.AddSingleton<ApiKeyAuthorizationFilter>();
    }
    
    private static void RegisterIdentity(IServiceCollection services)
    {
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

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
        services.AddSingleton<IHttpContextService, HttpContextService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<ITaskListService, TaskListService>();
        services.AddTransient<ITaskService, TaskService>();
    }

    private static void RegisterOthers(IServiceCollection services)
    {
        
    }
}