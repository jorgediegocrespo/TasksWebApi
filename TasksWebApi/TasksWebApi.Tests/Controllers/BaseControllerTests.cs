using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TasksWebApi.DataAccess;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.Models;
using TasksWebApi.Services;

namespace TasksWebApi.Tests.Controllers;

public class BaseControllerTests
{
    private WebApplicationFactory<Program> _factory;
    protected HttpClient _client;
    protected List<TaskListEntity> _dbTaskLists;
    protected List<TaskEntity> _dbTasks;
    
    protected async Task<WebApplicationFactory<Program>> BuildWebApplicationFactoryAsync(string dbContextName)
    {
        _factory = new WebApplicationFactory<Program>();
        _factory = _factory.WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

        await SeedDatabaseContextAsync(_factory);
        return _factory;
    }
    
    protected async Task DeleteDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TasksDbContext>();
        await context.Database.EnsureDeletedAsync();
    }
    
    protected void AddApiKeyHeader()
    {
        _client.DefaultRequestHeaders.Add("x-api-key", "testApiKey");
    }
    
    protected void RemoveApiKeyHeader()
    {
        _client.DefaultRequestHeaders.Remove("x-api-key");
    }
    
    protected void RemoveBearerTokenHeader()
    {
        _client.DefaultRequestHeaders.Authorization = null;
    }
    
    protected async Task<TokenResponse> UserSignIn()
    {
        var signInInfo = new SignInRequest("user", "!_-ABCabc123");
        StringContent content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync("/api/v1.0/user/signin", content);
        string serializedTokenInfo = await response.Content.ReadAsStringAsync();
        TokenResponse tokenInfo = JsonSerializer.Deserialize<TokenResponse>(serializedTokenInfo, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenInfo.Token);
        return tokenInfo;
    }
    
    protected async Task<TokenResponse> AdminSignIn()
    {
        var signInInfo = new SignInRequest("admin", "123abcABC-_!");
        StringContent content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync("/api/v1.0/user/signin", content);
        string serializedTokenInfo = await response.Content.ReadAsStringAsync();
        TokenResponse tokenInfo = JsonSerializer.Deserialize<TokenResponse>(serializedTokenInfo, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenInfo.Token);
        return tokenInfo;
    }
    
    protected async Task<TokenResponse> EmptySignIn()
    {
        var signInInfo = new SignInRequest("diego", "!_-ABCabc123");
        StringContent content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        HttpResponseMessage response = await _client.PostAsync("/api/v1.0/user/signin", content);
        string serializedTokenInfo = await response.Content.ReadAsStringAsync();
        TokenResponse tokenInfo = JsonSerializer.Deserialize<TokenResponse>(serializedTokenInfo, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenInfo.Token);
        return tokenInfo;
    }

    private async Task SeedDatabaseContextAsync(WebApplicationFactory<Program> webApplicationFactory)
    {
        using var scope = webApplicationFactory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetService<UserManager<UserEntity>>();
        var dbContextOptions = scope.ServiceProvider.GetRequiredService<DbContextOptions<TasksDbContext>>();
        var httpContextServiceMock = new Mock<IHttpContextService>();
        
        httpContextServiceMock
            .Setup(x => x.GetContextUser())
            .Returns(new UserResponse(Guid.NewGuid().ToString(), "user", "user@dicres.com", new List<string>()));
        var context = new TasksDbContext(httpContextServiceMock.Object, dbContextOptions);

        await InitializeDbForTestsAsync(context, roleManager, userManager);
    }

    private async Task InitializeDbForTestsAsync(TasksDbContext context, RoleManager<IdentityRole> roleManager, UserManager<UserEntity> userManager)
    {
        await context.Database.EnsureCreatedAsync();
        await AddInfoAsync(context, roleManager, userManager);
    }

    private async Task AddInfoAsync(TasksDbContext context, RoleManager<IdentityRole> roleManager, UserManager<UserEntity> userManager)
    {
        UserEntity user1 = new UserEntity
        {
            UserName = "diego",
            Email = "diego@diego.dicres"
        };
        await userManager.CreateAsync(user1, "!_-ABCabc123");
        
        UserEntity user2 = new UserEntity
        {
            UserName = "user",
            Email = "user@dicres.com"
        };
        await userManager.CreateAsync(user2, "!_-ABCabc123");
        string userId = await context.Users.Where(x => x.UserName == "user").Select(x => x.Id).FirstAsync();
        
        await context.TaskLists.AddRangeAsync(new List<TaskListEntity>
        {
            new() { Name = "List 1", RowVersion = new byte[] {0x01}, UserId = userId, Tasks = new List<TaskEntity>() //1
            {
                new() { Description = "Task 1", Notes = "This is the task 1"}, //1
                new() { Description = "Task 3", Notes = "This is the task 3"}, //2
                new() { Description = "Task 5" }, //3
                new() { Description = "Task 12", IsDeleted = true }, //4
                new() { Description = "Task 6", Notes = "This is the task 6"}, //5
                new() { Description = "Task 2", Notes = "This is the task 2"}, //6
                new() { Description = "Task 4", Notes = "This is the task 4"} //7
            } },
            new() { Name = "List 3", UserId = userId }, //2
            new() { Name = "List 4", UserId = userId, IsDeleted = true }, //3
            new()
            {
                Name = "List 2", UserId = userId, Tasks = new List<TaskEntity>() //4
                {
                    new() { Description = "Task 1", Notes = "This is the task 1"}, //8
                    new() { Description = "Task 2", Notes = "This is the task 2"}, //9
                }
            }
        });
        await context.SaveChangesAsync();
        
        _dbTaskLists = await context.TaskLists.ToListAsync();
        _dbTasks = await context.Tasks.ToListAsync();
        context.ChangeTracker.Clear();
    }
}