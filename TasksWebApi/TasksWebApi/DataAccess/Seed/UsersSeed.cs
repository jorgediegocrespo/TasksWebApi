using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.Models;

namespace TasksWebApi.DataAccess.Seed;

public class UsersSeed
{
    private readonly UserManager<UserEntity> _userManager;

    public UsersSeed(UserManager<UserEntity> userManager)
    {
        _userManager = userManager;
    }

    public async Task SeedAsync(TasksDbContext context, CancellationToken cancellationToken = default)
    {
        await AddDefaultUsersAsync(context, cancellationToken);
    }
    
    private async Task AddDefaultUsersAsync(TasksDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Users.AnyAsync(cancellationToken))
            return;

        var superAdmin = new UserEntity
        {
            UserName = "admin",
            Email = "admin@dicres.com"
        };
        await _userManager.CreateAsync(superAdmin, "123abcABC-_!");
        await _userManager.AddToRoleAsync(superAdmin, Roles.SUPERADMIN);
    }
}