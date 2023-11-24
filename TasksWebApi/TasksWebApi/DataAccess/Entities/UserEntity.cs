using Microsoft.AspNetCore.Identity;

namespace TasksWebApi.DataAccess.Entities;

public sealed class UserEntity : IdentityUser
{
    public string RefreshToken { get; set; }
    public DateTime RefreshTokenExpiration { get; set; }
}