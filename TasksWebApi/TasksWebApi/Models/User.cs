using System.ComponentModel.DataAnnotations;

namespace TasksWebApi.Models;

public abstract class Roles
{
    public const string SUPERADMIN = "SuperAdmin";
}

public record SignInRequest
{
    [Required]
    public string UserName { get; init; }

    [Required]
    [MinLength(12)]
    public string Password { get; init; }

    public SignInRequest(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }
}

public record SignUpRequest
{
    [Required]
    public string UserName { get; init; }

    [Required]
    [EmailAddress]
    public string Email { get; init; }

    [Required]
    [MinLength(12)]
    public string Password { get; init; }

    public SignUpRequest(string userName, string email, string password)
    {
        UserName = userName;
        Email = email;
        Password = password;
    }
}

public record UserDeleteRequest
{
    public string UserName { get; init; }

    public UserDeleteRequest(string userName)
    {
        UserName = userName;
    }
}

public record RefreshTokenRequest
{
    [Required]
    public string Token { get; init; }
    
    [Required]
    public string RefreshToken { get; init; }

    public RefreshTokenRequest(string token, string refreshToken)
    {
        Token = token;
        RefreshToken = refreshToken;
    }
}

public record UserResponse
{
    public string Id { get; init; }
    public string Name { get; init; }
    public string Email { get; init; }
    public List<string> Roles { get; init; }

    public UserResponse(string id, string name, string email, List<string> roles)
    {
        Id = id;
        Name = name;
        Email = email;
        Roles = roles;
    }
}

public record TokenResponse
{
    public string Token { get; init; }
    public string RefreshToken { get; init; }

    public TokenResponse(string token, string refreshToken)
    {
        Token = token;
        RefreshToken = refreshToken;
    }
}