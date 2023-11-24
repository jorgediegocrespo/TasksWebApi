using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TasksWebApi.DataAccess;

namespace TasksWebApi.Tests.DataAccess.Repositories;

public class BaseRepositoryTests
{
    protected async Task<TasksDbContext> GetLocalTasksDbContextAsync(string dbContextName)
    {
        string testAppSettingJson = await File.ReadAllTextAsync(@"./testappsettings.json");
        var testAppSetting = JsonSerializer.Deserialize<TestAppSettings>(testAppSettingJson);

        string connectionString = string.Format(testAppSetting.ConnectionStrings.DataBaseConnection, dbContextName);
        DbContextOptions<TasksDbContext> options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseSqlServer(connectionString, options => options.UseNetTopologySuite())
            .Options;

        var dbContext = CreateDatabaseContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        return dbContext;
    }

    protected async Task DeleteDatabaseAsync(TasksDbContext taskDbContext)
    {
        await taskDbContext.Database.EnsureDeletedAsync();
    }

    private TasksDbContext CreateDatabaseContext(DbContextOptions<TasksDbContext> options)
    {
        return new TasksDbContext(options);
    }
}