using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TasksWebApi.Constants;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Repositories;
using TasksWebApi.Exceptions;
using TasksWebApi.Mappers;
using TasksWebApi.Models;

namespace TasksWebApi.Services;

public class UserService : IUserService
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextService _httpContextService;
    private readonly ITaskListRepository _taskListRepository;

    public UserService(UserManager<UserEntity> userManager, SignInManager<UserEntity> signInManager, IConfiguration configuration, IHttpContextService httpContextService, ITaskListRepository taskListRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _httpContextService = httpContextService;
        _taskListRepository = taskListRepository;
    }
    
    public async Task<TokenResponse> SignUpAsync(SignUpRequest userInfo, CancellationToken cancellationToken = default)
    {
        UserEntity user = userInfo.ToIdentityUser();
        var result = await _userManager.CreateAsync(user, userInfo.Password);
        if (!result.Succeeded)
            return null;

        return await GetTokenInfoAsync(user.UserName);
    }

    public async Task<TokenResponse> SignInAsync(SignInRequest userInfo, CancellationToken cancellationToken = default)
    {
        SignInResult result = await _signInManager.PasswordSignInAsync(userInfo.UserName, userInfo.Password, false, false);
        return result.Succeeded ? await GetTokenInfoAsync(userInfo.UserName) : null;
    }

    public async Task DeleteAsync(UserDeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        UserEntity user = await _userManager.FindByNameAsync(deleteRequest.UserName);
        await ValidateEntityToDeleteAsync(user, cancellationToken);
        
        await _userManager.DeleteAsync(user!);
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken = default)
    {
        var userName = GetUserNameFromToken(refreshTokenRequest);
        if (userName == null) 
            return null;

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null ||
            user.RefreshToken != refreshTokenRequest.RefreshToken ||
            user.RefreshTokenExpiration <= DateTime.UtcNow)
            return null;
        
        return await GetTokenInfoAsync(user.UserName);
    }

    private string GetUserNameFromToken(RefreshTokenRequest refreshTokenRequest)
    {
        try
        {
            // Extract the user name from the token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(refreshTokenRequest.Token);
            if (jwtToken == null)
                return null;

            var userNameClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name);
            if (userNameClaim == null)
                return null;
        
            return userNameClaim.Value;
        }
        catch (Exception)
        {
            // Token is not valid
            return null;
        }
    }

    private async Task ValidateEntityToDeleteAsync(UserEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new NotValidOperationException(ErrorCodes.ITEM_NOT_EXISTS, $"The user to remove does not exists");

        var contextUser = _httpContextService.GetContextUser();
        
        //Only superadmin can delete other users
        if (!entity.Id.Equals(contextUser.Id, StringComparison.InvariantCultureIgnoreCase) &&
            contextUser.Roles?.Contains(Roles.SUPERADMIN) != true)
            throw new ForbidenActionException();
        
        //A superadmin can not be deleted
        if (await _userManager.IsInRoleAsync(entity, Roles.SUPERADMIN))
            throw new ForbidenActionException();
        
        //User can not be deleted if it has any list
        //As the app use soft delete, if the user has any list deleted (IsDeleted = true) it neither can be deleted
        int userTaskListCount = await _taskListRepository.GetTotalRecordsAsync(entity.Id, cancellationToken, true);
        if (userTaskListCount > 0)
            throw new NotValidOperationException(ErrorCodes.USER_WITH_LISTS, $"The user to remove owns any task list");
    }

    private async Task<TokenResponse> GetTokenInfoAsync(string userName)
    {
        int expirationMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"]!);
        var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);
        
        var token = await GenerateTokenAsync(userName, expiration);
        var user = await GenerateRefreshTokenAsync(userName);
        
        var tokenInfo = new TokenResponse(token, user.RefreshToken);
        return tokenInfo;
    }
    
    private async Task<string> GenerateTokenAsync(string userName, DateTime expiration)
    {
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var key = _configuration["Jwt:Key"];
        ;
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!));
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        IList<Claim> claims = await GetUserClaimsAsync(userName);
        
        JwtSecurityToken securityToken = new JwtSecurityToken(
            issuer: issuer, 
            audience: audience, 
            claims: claims, 
            expires: expiration, 
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
    
    private async Task<UserEntity> GenerateRefreshTokenAsync(string userName, CancellationToken cancellationToken = default)
    {
        var refreshTokenExpireMinutes = int.Parse(_configuration["Jwt:RefreshTokenExpireMinutes"]!);
        
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);
        
        UserEntity user = await _userManager.FindByNameAsync(userName);
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiration = DateTime.UtcNow.AddMinutes(refreshTokenExpireMinutes);
        
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return null;

        return user;
    }

    private async Task<IList<Claim>> GetUserClaimsAsync(string userName)
    {
        UserEntity user = await _userManager.FindByNameAsync(userName);
        IList<string> roles = await _userManager.GetRolesAsync(user!);
        IList<Claim> claims = await _userManager.GetClaimsAsync(user);

        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
        claims.Add(new Claim(ClaimTypes.Name, user.UserName));
        claims.Add(new Claim(ClaimTypes.Email, user.Email));            

        foreach (var rol in roles)
            claims.Add(new Claim(ClaimTypes.Role, rol));

        return claims;
    }
}