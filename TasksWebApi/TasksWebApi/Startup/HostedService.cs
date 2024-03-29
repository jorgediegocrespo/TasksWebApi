using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TasksWebApi.DataAccess;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Seed;

namespace TasksWebApi.Startup;

public class HostedService(IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetService<TasksDbContext>();
        await context!.Database.MigrateAsync(cancellationToken);
        
        var userManager = services.GetService<UserManager<UserEntity>>();
        await new UsersSeed(userManager).SeedAsync(context, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}