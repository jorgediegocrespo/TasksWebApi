using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TasksWebApi.Constants;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Repositories;
using TasksWebApi.Exceptions;
using TasksWebApi.Models;
using TasksWebApi.Services;

namespace TasksWebApi.Tests.Services;

[TestClass]
public class UserServiceTests
{
    private Mock<UserManager<UserEntity>> _userManagerMock;
    private Mock<SignInManager<UserEntity>> _signInManagerMock;
    private Mock<IHttpContextService> _httpContextServiceMock;
    private Mock<ITaskListRepository> _taskListRepositoryMock;
    private UserService _userService;

    public UserServiceTests()
    {
        _userManagerMock = new Mock<UserManager<UserEntity>>(Mock.Of<IUserStore<UserEntity>>(), null, null, null, null, null, null, null, null);
        _httpContextServiceMock = new Mock<IHttpContextService>();
        _taskListRepositoryMock = new Mock<ITaskListRepository>();
        var userClaimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<UserEntity>>();
        var identityOptionsMock = new Mock<IOptions<IdentityOptions>>();
        var loggerMock = new Mock<ILogger<SignInManager<UserEntity>>>();
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
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

        _taskListRepositoryMock
            .Setup(x => x.GetTotalRecordsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
            .ReturnsAsync(0);
        
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

        _userService = new UserService(_userManagerMock.Object, _signInManagerMock.Object, configuration, _httpContextServiceMock.Object, _taskListRepositoryMock.Object);
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
        
        SignUpRequest userInfo = new SignUpRequest(userName, "user@user.com", "password");
        if (userName != "user1")
        {
            TokenResponse result = await _userService.SignUpAsync(userInfo);
            Assert.IsNull(result);
        }
        else
        {
            TokenResponse result = await _userService.SignUpAsync(userInfo);
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Token));
        }
    }
    
    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task sign_in(bool correctSignIn)
    {
        string userName = correctSignIn ? "user1" : "user2";
        string password = correctSignIn ? "!_-ABCabc123" : "123abcABC_-!";

        _signInManagerMock
            .Setup(x => x.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),
                It.IsAny<bool>()))
            .ReturnsAsync((string userName, string password, bool isPersistent, bool lockoutOnFailure) =>
            {
                if (userName == "user1" && password == "!_-ABCabc123")
                    return SignInResult.Success;

                return SignInResult.Failed;
            });
        
        TokenResponse tokenInfo = await _userService.SignInAsync(new SignInRequest(userName, password));
        Assert.AreEqual(correctSignIn, !string.IsNullOrWhiteSpace(tokenInfo?.Token));
    }
    
    [TestMethod]
    [DataRow(true, true)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    [DataRow(false, false)]
    public async Task delete_other(bool contextUserAdmin, bool userToRemoveAdmin)
    {
        _httpContextServiceMock
            .Setup(x => x.GetContextUser())
            .Returns(() =>
            {
                if (contextUserAdmin)
                    return new UserResponse("1", "user1", "user1@user1.com", new List<string>() { Roles.SUPERADMIN });
                
                return new UserResponse("2", "user2", "user2@user2.com", new List<string>());
            });

        UserDeleteRequest deleteRequest;
        if (userToRemoveAdmin)
            deleteRequest = new UserDeleteRequest("user1");
        else
            deleteRequest = new UserDeleteRequest("user2");
        
        if (contextUserAdmin && !userToRemoveAdmin)
        {
            await _userService.DeleteAsync(deleteRequest);
            Assert.IsTrue(true);
        }
        else 
            await Assert.ThrowsExceptionAsync<ForbidenActionException>(() => _userService.DeleteAsync(deleteRequest));
    }
}