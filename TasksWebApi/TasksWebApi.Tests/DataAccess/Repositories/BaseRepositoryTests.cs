using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Moq;
using TasksWebApi.DataAccess;
using TasksWebApi.Models;
using TasksWebApi.Services;

namespace TasksWebApi.Tests.DataAccess.Repositories;

public class BaseRepositoryTests
{
    private readonly Mock<IHttpContextService> _httpContextServiceMock;

    public BaseRepositoryTests()
    {
        _httpContextServiceMock = new Mock<IHttpContextService>();
    }
    
    protected async Task<TasksDbContext> GetLocalTasksDbContextAsync(string dbContextName)
    {
        _httpContextServiceMock
            .Setup(x => x.GetContextUser())
            .Returns(new UserResponse(Guid.NewGuid().ToString(), "user1", "user1@dicres.com", new List<string>()));
        
        string testAppSettingJson = await File.ReadAllTextAsync(@"./DataAccess/Repositories/repositorySettings.json");
        RepositorySettings testAppSetting = JsonSerializer.Deserialize<RepositorySettings>(testAppSettingJson);

        string connectionString = string.Format(testAppSetting.RepositoryTestConnectionString, dbContextName);
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
        return new TasksDbContext(_httpContextServiceMock.Object, options);
    }
}