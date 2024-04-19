using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TasksWebApi.Constants;
using TasksWebApi.DataAccess;
using TasksWebApi.DataAccess.Entities;
using TasksWebApi.DataAccess.Repositories;
using TasksWebApi.Exceptions;
using TasksWebApi.Mappers;
using TasksWebApi.Models;

namespace TasksWebApi.Services;

public class UserService(
    UserManager<UserEntity> userManager,
    SignInManager<UserEntity> signInManager,
    IConfigurationValuesService configurationValuesService,
    IHttpContextService httpContextService,
    ITaskListRepository taskListRepository,
    IUnitOfWork unitOfWork,
    ILogger<UserService> loggerManager)
    : IUserService
{
    public async Task<TokenResponse> SignUpAsync(SignUpRequest userInfo, CancellationToken cancellationToken = default)
    {
        var user = userInfo.ToIdentityUser();
        var result = await userManager.CreateAsync(user, userInfo.Password);
        if (result.Succeeded) 
            return await GetTokenInfoAsync(user.UserName);
        
        loggerManager.LogInformation("User creation failed");
        return null;
    }

    public async Task<TokenResponse> SignInAsync(SignInRequest userInfo, CancellationToken cancellationToken = default)
    {
        var result = await signInManager.PasswordSignInAsync(userInfo.UserName, userInfo.Password, false, false);
        if (result.Succeeded)
            return await GetTokenInfoAsync(userInfo.UserName);
        
        loggerManager.LogInformation("User login failed");
        return null;
    }

    public async Task DeleteAsync(UserDeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByNameAsync(deleteRequest.UserName);
        await ValidateEntityToDeleteAsync(user, false, cancellationToken);
        await userManager.DeleteAsync(user!);
    }
    
    public async Task DeleteWithDataAsync(UserDeleteRequest deleteRequest, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByNameAsync(deleteRequest.UserName);
        await ValidateEntityToDeleteAsync(user, true, cancellationToken);
        
        await unitOfWork.SaveChangesInTransactionAsync(
            async () =>
            {
                await taskListRepository.DeleteAsync(user!.Id, true, cancellationToken);
                await unitOfWork.SaveChangesWithoutSoftDeleteAsync(cancellationToken);
                await userManager.DeleteAsync(user!);
            }, cancellationToken);
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest, CancellationToken cancellationToken = default)
    {
        var userName = GetUserNameFromToken(refreshTokenRequest);
        if (userName == null)
        {
            loggerManager.LogInformation("Token is not valid");
            return null;
        }

        var user = await userManager.FindByNameAsync(userName);
        if (user == null ||
            user.RefreshToken != refreshTokenRequest.RefreshToken ||
            user.RefreshTokenExpiration <= DateTime.UtcNow)
        {
            loggerManager.LogInformation("Refresh token is not valid");
            return null;
        }

        return await GetTokenInfoAsync(user.UserName);
    }

    private string GetUserNameFromToken(RefreshTokenRequest refreshTokenRequest)
    {
        try
        {
            // Extract the user name from the token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(refreshTokenRequest.Token);
            var userNameClaim = jwtToken?.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name);
            return userNameClaim?.Value;
        }
        catch (Exception)
        {
            // Token is not valid
            return null;
        }
    }

    private async Task ValidateEntityToDeleteAsync(UserEntity entity, bool willRemoveData, CancellationToken cancellationToken = default)
    {
        if (entity == null)
        {
            loggerManager.LogInformation("The user to remove does not exists");
            throw new NotValidOperationException(ErrorCodes.ITEM_NOT_EXISTS, "The user to remove does not exists");
        }

        var contextUser = httpContextService.GetContextUser();
        
        //Only superadmin can delete other users
        if (!entity.Id.Equals(contextUser.Id, StringComparison.InvariantCultureIgnoreCase) &&
            contextUser.Roles?.Contains(Roles.SUPERADMIN) != true)
        {
            loggerManager.LogInformation("Only superadmin can delete other users");
            throw new ForbidenActionException();
        }

        //A superadmin can not be deleted
        if (await userManager.IsInRoleAsync(entity, Roles.SUPERADMIN))
        {
            loggerManager.LogInformation("A superadmin can not be deleted");
            throw new ForbidenActionException();
        }

        //User can not be deleted if it has any list
        //As the app use soft delete, if the user has any list deleted (IsDeleted = true) it neither can be deleted
        var removedUserTaskListCount = await taskListRepository.GetTotalRecordsAsync(entity.Id, cancellationToken, !willRemoveData);
        if (removedUserTaskListCount > 0)
        {
            loggerManager.LogInformation("The user to remove owns some task list");
            throw new NotValidOperationException(ErrorCodes.USER_WITH_LISTS, "The user to remove owns some task list");
        }
    }

    private async Task<TokenResponse> GetTokenInfoAsync(string userName)
    {
        var jwtSettings = await configurationValuesService.GetJwtSettingsAsync();
        var expirationMinutes = jwtSettings.ExpireMinutes;
        var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);
        
        var token = await GenerateTokenAsync(jwtSettings, userName, expiration);
        var user = await GenerateRefreshTokenAsync(jwtSettings, userName);
        
        var tokenInfo = new TokenResponse(token, user.RefreshToken);
        return tokenInfo;
    }
    
    private async Task<string> GenerateTokenAsync(JwtSettings jwtSettings, string userName, DateTime expiration)
    { ;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = await GetUserClaimsAsync(userName);
        
        var securityToken = new JwtSecurityToken(
            issuer: jwtSettings.Issuer, 
            audience: jwtSettings.Audience, 
            claims: claims, 
            expires: expiration, 
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
    
    private async Task<UserEntity> GenerateRefreshTokenAsync(JwtSettings jwtSettings, string userName, CancellationToken cancellationToken = default)
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var refreshToken = Convert.ToBase64String(randomNumber);
        
        var user = await userManager.FindByNameAsync(userName);
        user!.RefreshToken = refreshToken;
        user!.RefreshTokenExpiration = DateTime.UtcNow.AddMinutes(jwtSettings.RefreshTokenExpireMinutes);
        
        var result = await userManager.UpdateAsync(user);
        if (result.Succeeded) 
            return user;
        
        loggerManager.LogInformation("User update failed");
        return null;
    }

    private async Task<IList<Claim>> GetUserClaimsAsync(string userName)
    {
        var user = await userManager.FindByNameAsync(userName);
        var roles = await userManager.GetRolesAsync(user!);
        var claims = await userManager.GetClaimsAsync(user);

        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
        claims.Add(new Claim(ClaimTypes.Name, user.UserName));
        claims.Add(new Claim(ClaimTypes.Email, user.Email));            

        foreach (var rol in roles)
            claims.Add(new Claim(ClaimTypes.Role, rol));

        return claims;
    }
}