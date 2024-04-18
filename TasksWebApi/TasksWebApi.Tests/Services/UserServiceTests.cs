using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TasksWebApi.DataAccess;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Repositories;
using TasksWebApi.Exceptions;
using TasksWebApi.Models;
using TasksWebApi.Services;

namespace TasksWebApi.Tests.Services;

[TestClass]
public class UserServiceTests
{
    private readonly Mock<UserManager<UserEntity>> _userManagerMock;
    private readonly Mock<SignInManager<UserEntity>> _signInManagerMock;
    private readonly Mock<IHttpContextService> _httpContextServiceMock;
    private readonly Mock<ITaskListRepository> _taskListRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UserService>> _loggerUserServiceMock;
    private UserService _userService;

    public UserServiceTests()
    {
        _userManagerMock = new Mock<UserManager<UserEntity>>(Mock.Of<IUserStore<UserEntity>>(), null, null, null, null, null, null, null, null);
        _httpContextServiceMock = new Mock<IHttpContextService>();
        _taskListRepositoryMock = new Mock<ITaskListRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        
        var userClaimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<UserEntity>>();
        var identityOptionsMock = new Mock<IOptions<IdentityOptions>>();
        var loggerMock = new Mock<ILogger<SignInManager<UserEntity>>>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerUserServiceMock = new Mock<ILogger<UserService>>();
        _signInManagerMock = new Mock<SignInManager<UserEntity>>(_userManagerMock.Object, httpContextAccessorMock.Object, userClaimsPrincipalFactoryMock.Object, identityOptionsMock.Object, loggerMock.Object, null, null);

    }
    
    [TestInitialize]
    public Task InitializeAsync()
    {
        _userManagerMock
            .Setup(x => x.GetUserIdAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync((UserEntity user) =>
            {
                if (user.UserName == "user1")
                    return "1";

                return null;
            });

        _userManagerMock
            .Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) =>
            {
                if (id == "user1")
                    return new UserEntity { Id = "1", UserName = "user1", Email = "user1@user.com" };

                return new UserEntity { Id = $"{id}", UserName = $"user{id}", Email = $"user{id}@user{id}.com" };
            });

        _userManagerMock
            .Setup(x => x.UpdateAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync((UserEntity user) => IdentityResult.Success);

        _userManagerMock
            .Setup(x => x.DeleteAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync((UserEntity user) =>
            {
                if (user.UserName == "user1")
                    return IdentityResult.Success;

                return IdentityResult.Failed();
            });

        _userManagerMock
            .Setup(x => x.GetRolesAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync((UserEntity user) =>
            {
                if (user.UserName == "user1")
                    return new List<string>() { Roles.SUPERADMIN };

                return null;
            });
        
        _userManagerMock
            .Setup(x => x.IsInRoleAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
            .ReturnsAsync((UserEntity user, string role) =>
            {
                if (!string.Equals(Roles.SUPERADMIN, role, StringComparison.InvariantCultureIgnoreCase))
                    return false;

                return string.Equals(user.UserName, "user1", StringComparison.InvariantCultureIgnoreCase);
            });

        _userManagerMock
            .Setup(x => x.GetClaimsAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync(new List<Claim>());

        _unitOfWorkMock
            .Setup(x => x.SaveChangesInTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var inMemorySettings = new Dictionary<string, string> {
            {"Jwt:Issuer", "test"},
            {"Jwt:Audience", "test"},
            {"Jwt:Key", "xcCO6eJZoiW5gzl8nJ2HrwH2UHKjpJoTQGtd6dzF74pMpRTJyy"},
            {"Jwt:ExpireMinutes", "1"},
            {"Jwt:RefreshTokenExpireMinutes", "2"},
            //...populate as needed for the test
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        var appSettingsConfigurationValuesService = new AppSettingsConfigurationValuesService(configuration);
        _userService = new UserService(_userManagerMock.Object, _signInManagerMock.Object, appSettingsConfigurationValuesService, _httpContextServiceMock.Object, _taskListRepositoryMock.Object, _unitOfWorkMock.Object, _loggerUserServiceMock.Object);
        return Task.CompletedTask;
    }
    
    [TestMethod]
    [DataRow("user1")]
    [DataRow("user2")]
    public async Task sign_up(string userName)
    {
        _userManagerMock
            .Setup(x => x.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<string>()))
            .ReturnsAsync((UserEntity user, string password) =>
            {
                if (user.UserName == "user1")
                    return IdentityResult.Success;

                return IdentityResult.Failed();
            });
        
        var userInfo = new SignUpRequest(userName, "user@user.com", "password");
        if (userName != "user1")
        {
            var result = await _userService.SignUpAsync(userInfo);
            Assert.IsNull(result);
        }
        else
        {
            var result = await _userService.SignUpAsync(userInfo);
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Token));
        }
    }
    
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task sign_in(bool correctSignIn)
    {
        var userName = correctSignIn ? "user1" : "user2";
        var password = correctSignIn ? "!_-ABCabc123" : "123abcABC_-!";

        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<bool>()))
            .ReturnsAsync((string userName, string password, bool isPersistent, bool lockoutOnFailure) =>
            {
                if (userName == "user1" && password == "!_-ABCabc123")
                    return SignInResult.Success;

                return SignInResult.Failed;
            });
        
        var tokenInfo = await _userService.SignInAsync(new SignInRequest(userName, password));
        Assert.AreEqual(correctSignIn, !string.IsNullOrWhiteSpace(tokenInfo?.Token));
    }
    
    [TestMethod]
    [DataRow(true, true, false)]
    [DataRow(true, false, false)]
    [DataRow(false, true, false)]
    [DataRow(false, false, false)]
    [DataRow(true, true, true)]
    [DataRow(true, false, true)]
    [DataRow(false, true, true)]
    [DataRow(false, false, true)]
    public async Task delete(bool contextUserAdmin, bool userToRemoveAdmin, bool hasAnyList)
    {
        _httpContextServiceMock
            .Setup(x => x.GetContextUser())
            .Returns(() => contextUserAdmin ? 
                new UserResponse("1", "user1", "user1@user1.com", new List<string>() { Roles.SUPERADMIN }) : 
                new UserResponse("2", "user2", "user2@user2.com", new List<string>()));
        
        _taskListRepositoryMock
            .Setup(x => x.GetTotalRecordsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(hasAnyList ? 1 : 0);

        var deleteRequest = userToRemoveAdmin ? new UserDeleteRequest("user1") : new UserDeleteRequest("user2");
        if (contextUserAdmin && !userToRemoveAdmin && !hasAnyList)
        {
            await _userService.DeleteAsync(deleteRequest);
            Assert.IsTrue(true);
        }
        else if (!contextUserAdmin || userToRemoveAdmin)
            await Assert.ThrowsExceptionAsync<ForbidenActionException>(() => _userService.DeleteAsync(deleteRequest));
        else
            await Assert.ThrowsExceptionAsync<NotValidOperationException>(() => _userService.DeleteAsync(deleteRequest));
    }
    
    [TestMethod]
    [DataRow(true, true, false)]
    [DataRow(true, false, false)]
    [DataRow(false, true, false)]
    [DataRow(false, false, false)]
    [DataRow(true, true, true)]
    [DataRow(true, false, true)]
    [DataRow(false, true, true)]
    [DataRow(false, false, true)]
    public async Task delete_with_data(bool contextUserAdmin, bool userToRemoveAdmin, bool hasAnyList)
    {
        _httpContextServiceMock
            .Setup(x => x.GetContextUser())
            .Returns(() => contextUserAdmin ? 
                new UserResponse("1", "user1", "user1@user1.com", new List<string>() { Roles.SUPERADMIN }) : 
                new UserResponse("2", "user2", "user2@user2.com", new List<string>()));
        
        _taskListRepositoryMock
            .Setup(x => x.GetTotalRecordsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(hasAnyList ? 1 : 0);

        var deleteRequest = userToRemoveAdmin ? new UserDeleteRequest("user1") : new UserDeleteRequest("user2");
        if (contextUserAdmin && !userToRemoveAdmin && !hasAnyList)
        {
            await _userService.DeleteWithDataAsync(deleteRequest);
            Assert.IsTrue(true);
        }
        else if (!contextUserAdmin || userToRemoveAdmin)
            await Assert.ThrowsExceptionAsync<ForbidenActionException>(() => _userService.DeleteAsync(deleteRequest));
        else
            await Assert.ThrowsExceptionAsync<NotValidOperationException>(() => _userService.DeleteAsync(deleteRequest));
    }
}